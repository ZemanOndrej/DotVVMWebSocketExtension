var uri = "ws://" + window.location.host + "/ws";

function connect() {
	var socket = new WebSocket(uri);
	socket.onopen = function(event) {
		console.log("opened connection to " + uri + "; ");
		console.log("connected", event.data);
	};
	socket.onclose = function(event) {
		console.log("closed connection from " + uri);
	};
	socket.onmessage = function(event) {
//		console.log("onmessage", event);

		var viewModelName = "root";//TODO
		var resultObject = JSON.parse(event.data);

		console.log("RootViewModel", Object.keys(dotvvm.viewModels));
		console.log("Root", Object.keys(dotvvm.viewModelObservables));
		console.log(resultObject, "result");

		if (resultObject.type) {

			dotvvm.viewModelObservables[viewModelName]().Hub().SocketId(resultObject.socketId);
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
	
}

function sleep(milliseconds) {
	var start = new Date().getTime();
	for (var i = 0; i < 1e7; i++) {
		if ((new Date().getTime() - start) > milliseconds) {
			break;
		}
	}
}

connect();