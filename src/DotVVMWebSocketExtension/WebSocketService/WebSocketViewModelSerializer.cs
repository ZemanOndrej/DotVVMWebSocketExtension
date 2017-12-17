using System;
using System.Collections.Generic;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Security;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public class WebSocketViewModelSerializer
	{
		private const string GeneralViewModelRecommendations =
			"Check out general viewModel recommendation at http://www.dotvvm.com/docs/tutorials/basics-viewmodels.";

		protected readonly IViewModelSerializationMapper Mapper;

		public Formatting JsonFormatting { get; set; }


		public WebSocketViewModelSerializer(IViewModelSerializationMapper mapper)
		{
			Mapper = mapper;
			JsonFormatting = Formatting.None;
		}

		/// <summary>
		/// Builds the view model.
		/// </summary>
		/// <param name="state">The state.</param>
		/// <exception cref="Exception"></exception>
		public void BuildViewModel(ViewModelState state)
		{
			var jsonSerializer = CreateJsonSerializer();
			var viewModelConverter = new ViewModelJsonConverter(true, Mapper)
			{
				UsedSerializationMaps = new HashSet<ViewModelSerializationMap>()
			};
			jsonSerializer.Converters.Add(viewModelConverter);
			var writer = new JTokenWriter();
			try
			{
				jsonSerializer.Serialize(writer, state.LastSentViewModel);
			}
			catch (Exception ex)
			{
				throw new Exception(
					$"Could not serialize viewModel of type {state.LastSentViewModel.GetType().Name}. Serialization failed at property {writer.Path}.",
					ex);
			}

			writer.Token["$csrfToken"] = state.CsrfToken;


			var result = new JObject();
			result["viewModel"] = writer.Token;
			result["action"] = WebSocketRequestType.SuccessfulCommand;
			state.ChangedViewModelJson = result;
		}

		protected virtual JsonSerializer CreateJsonSerializer() => CreateDefaultSettings().Apply(JsonSerializer.Create);

		/// <summary>
		/// Creates the default settings for Serializer.
		/// </summary>
		/// <returns></returns>
		public static JsonSerializerSettings CreateDefaultSettings()
		{
			var s = new JsonSerializerSettings
			{
				DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
			};
			s.Converters.Add(new DotvvmDateTimeConverter());
			s.Converters.Add(new StringEnumConverter());
			return s;
		}

		/// <summary>
		/// Serializes the view model.
		/// </summary>
		/// <param name="state">The state.</param>
		/// <returns></returns>
		public string SerializeViewModel(ViewModelState state)
		{
			if (state.LastSentViewModelJson != null && state.ChangedViewModelJson["viewModel"] != null)
			{
				state.ChangedViewModelJson?.Remove("viewModelDiff");

				state.ChangedViewModelJson["viewModelDiff"] = JsonUtils.Diff(
					(JObject) state.LastSentViewModelJson["viewModel"],
					(JObject) state.ChangedViewModelJson["viewModel"], true);


				state.LastSentViewModelJson["viewModel"] = (JObject) state.ChangedViewModelJson["viewModel"].DeepClone();

				state.ChangedViewModelJson.Remove("viewModel");
			}
			return state.ChangedViewModelJson.ToString(JsonFormatting);
		}

		/// <summary>
		/// Populates the view model from Json sent.
		/// </summary>
		/// <param name="state">The state.</param>
		/// <param name="serializedPostData">The serialized post data.</param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public string PopulateViewModel(ViewModelState state, string serializedPostData)
		{
			// get properties
			var data = state.LastSentViewModelJson = JObject.Parse(serializedPostData);
			var viewModelToken = (JObject) data["viewModel"];

			// load CSRF token
			state.CsrfToken = viewModelToken["$csrfToken"].Value<string>();

			var viewModelConverter = new ViewModelJsonConverter(true, Mapper);

			// populate the ViewModel
			var serializer = CreateJsonSerializer();
			serializer.Converters.Add(viewModelConverter);
			try
			{
				viewModelConverter.Populate(viewModelToken.CreateReader(), serializer, state.LastSentViewModel);
			}
			catch (Exception ex)
			{
				throw new Exception(
					$"Could not deserialize viewModel of type {state.LastSentViewModel.GetType().Name}. {GeneralViewModelRecommendations}",
					ex);
			}
			return (string) data["taskId"];
		}
	}
}