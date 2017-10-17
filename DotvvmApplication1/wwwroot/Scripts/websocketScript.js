var uri = "ws://" + window.location.host + "/ws";

function connect() {
	socket = new WebSocket(uri);
	socket.onopen = function(event) {
		console.log("opened connection to " + uri);
	};
	socket.onclose = function(event) {
		console.log("closed connection from " + uri);
	};
	socket.onmessage = function(result) {
		console.log(result);
		var viewModelName = "root";
		try {

			var resultObject = JSON.parse(result.data);


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
		} catch (error) {
			console.log(error);
		}
	}
}

connect();