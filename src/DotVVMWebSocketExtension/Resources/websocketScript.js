var initWs = function() {
	const messageEnum = {
		Init: "webSocketInit",
		Sync: "viewModelSynchronizationRequest",
		Change: "successfulCommand",
		Ping: "ping"
	}

	var viewModelName = "root";
	var uri = `ws://${window.location.host}${dotvvm.viewModelObservables[viewModelName]().Service().Path()}`;
	var autoReconnectInterval = 5 * 1000;

	function connect() {
		var socket = new WebSocket(uri);
		dotvvm.websocket = socket;

		socket.onopen = function(event) {
		};
		socket.onclose = function (event) {
			console.log(`WebSocketClient: retry in ${autoReconnectInterval}ms`, event);
			setTimeout(function () {
				console.log("WebSocketClient: reconnecting...");
				connect();
			}, autoReconnectInterval);
			console.error(`closed connection from ${uri}`, event);
		};
		socket.onmessage = function(event) {
			var resultObject = JSON.parse(event.data);
			switch (resultObject.action) {
			case messageEnum.Init:
				dotvvm.viewModelObservables[viewModelName]().Service().ConnectionId(resultObject.socketId);
				break;
			case messageEnum.Change:
				updateViewModel(resultObject);
				break;
			case messageEnum.Sync:

				var viewModel = dotvvm.viewModels[viewModelName].viewModel;
				var data = {
					viewModel: dotvvm.serialization.serialize(viewModel,
						{ pathMatcher: function(val) { return context && val === context.$data; } }),
					taskId: resultObject.taskId
				};
				socket.send(ko.toJSON(data));
				break;
			};
		};

		window.onbeforeunload = function() {
			socket.onclose = function() {
			};
			socket.close();
		};
	}

	function updateViewModel(resultObject) {

		if (!resultObject.viewModel && resultObject.viewModelDiff) {
			resultObject.viewModel = dotvvm.patch(dotvvm.serialization.serialize(dotvvm.viewModels[viewModelName].viewModel),
				resultObject.viewModelDiff);
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

	function reconnect(e) {
		console.log(`WebSocketClient: retry in ${autoReconnectInterval}ms`, e);
		setTimeout(function () {
			console.log("WebSocketClient: reconnecting...");
			connect();
		}, autoReconnectInterval);
	}

	connect();
};

dotvvm.events.init.subscribe((e) => initWs());