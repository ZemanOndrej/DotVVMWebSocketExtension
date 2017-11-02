

var initWs= ()=> {
	var uri = "ws://" + window.location.host + "/ws";
	var wsCount = 0;
	var viewModelName = "root";
	function connect() {
		var socket = new WebSocket(uri);
		socket.onopen = function (event) {
			console.log("opened connection to " + uri + "; ");
		};
		socket.onclose = function (event) {
			console.log("closed connection from " + uri);
		};
		socket.onmessage = function (event) {
			console.log("onmessage", event.data);
			wsCount++;
			console.log(wsCount);

			var resultObject = JSON.parse(event.data);
			if (resultObject.type) {

				dotvvm.viewModelObservables[viewModelName]().Hub().CurrentSocketId(resultObject.socketId);
				return;
			}


			if (!resultObject.viewModel && !resultObject.viewModelDiff) {
				console.log(resultObject);
				return;
			}


			if (!resultObject.viewModel && resultObject.viewModelDiff) {
				resultObject.viewModel = dotvvm.patch(dotvvm.viewModels[viewModelName].viewModel, resultObject.viewModelDiff);
			}

			dotvvm.loadResourceList(resultObject.resources,
				() => {
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
						return;
					}
				});

		}

		window.onbeforeunload = function () {
			socket.onclose = function () { }; // disable onclose handler first
			socket.close();
		};

	}

	connect();
}

dotvvm.events.init.subscribe((e) => initWs());
