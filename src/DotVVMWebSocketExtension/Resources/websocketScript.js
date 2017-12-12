var initWs = function() {
	var viewModelName = "root";
	var uri = "ws://" + window.location.host + dotvvm.viewModelObservables[viewModelName]().Hub().Path();
	var wsCount = 0;

	function connect() {
		var socket = new WebSocket(uri);
		dotvvm.websocket = socket;

		socket.onopen = function(event) {
			console.log("opened connection to " + uri + "; ");
		};
		socket.onclose = function(event) {
			console.log("closed connection from " + uri);
		};
		socket.onmessage = function(event) {
			console.log("onmessage", event.data);
			wsCount++;
			console.log(wsCount);

			var resultObject = JSON.parse(event.data);
			switch (resultObject.action) {
			case "webSocketInit":
				console.log(resultObject.type);
				dotvvm.viewModelObservables[viewModelName]().Hub().ConnectionId(resultObject.socketId);
				break;
			case "successfulCommand":
				updateViewModel(resultObject);
				break;
			case "viewModelSynchronizationRequest":

				var viewModel = dotvvm.viewModels[viewModelName].viewModel;
				var data = {
					viewModel: dotvvm.serialization.serialize(viewModel,
						{ pathMatcher: function(val) { return context && val === context.$data; } }),
				};

				socket.send(ko.toJSON(data));
				break;
			case "pong":
				console.log("pong message");

			};


		};

		window.onbeforeunload = function() {
			socket.onclose = function() {
			};
			socket.close();
		};

	}

	function updateViewModel(resultObject) {
		if (!resultObject.viewModel && !resultObject.viewModelDiff) {
			console.log(resultObject);
			return;
		}

		if (!resultObject.viewModel && resultObject.viewModelDiff) {
			resultObject.viewModel = dotvvm.patch(dotvvm.serialization.serialize(dotvvm.viewModels[viewModelName].viewModel), resultObject.viewModelDiff);
		}


		if (resultObject.action === "successfulCommand") {
			try {
				dotvvm.isViewModelUpdating = true;

				var updatedControls = dotvvm.cleanUpdatedControls(resultObject);

				if (resultObject.viewModel) {
					ko.delaySync.pause();
					dotvvm.serialization.deserialize(resultObject.viewModel, dotvvm.viewModels[viewModelName].viewModel);
					ko.delaySync.resume();
				}
				dotvvm.cleanUpdatedControls(resultObject, updatedControls);
				dotvvm.viewModelObservables.root.valueHasMutated();

				dotvvm.restoreUpdatedControls(resultObject, updatedControls, true);
			} finally {
				dotvvm.isViewModelUpdating = false;
			}
		} else if (resultObject.action === "redirect") {
			dotvvm.handleRedirect(resultObject, viewModelName);
		}

	}

	connect();
};

dotvvm.events.init.subscribe((e) => initWs());