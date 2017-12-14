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
		private const string GeneralViewModelRecommendations = "Check out general viewModel recommendation at http://www.dotvvm.com/docs/tutorials/basics-viewmodels.";

		protected readonly IViewModelSerializationMapper mapper;

		public Formatting JsonFormatting { get; set; }
		private readonly IViewModelProtector viewModelProtector;


		public WebSocketViewModelSerializer(IViewModelSerializationMapper mapper, IViewModelProtector viewModelProtector)
		{
			this.mapper = mapper;
			this.viewModelProtector = viewModelProtector;
			JsonFormatting = Formatting.None;
		}

		public void BuildViewModel(ViewModelState state)
		{
			var jsonSerializer = CreateJsonSerializer();
			var viewModelConverter = new ViewModelJsonConverter(true, mapper)
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
			result["action"] = "successfulCommand";
			state.ChangedViewModelJson = result;
		}


		protected virtual JsonSerializer CreateJsonSerializer() => CreateDefaultSettings().Apply(JsonSerializer.Create);


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

		public void PopulateViewModel(ViewModelState state, string serializedPostData)
		{

			// get properties
			var data = state.LastSentViewModelJson = JObject.Parse(serializedPostData);
			var viewModelToken = (JObject)data["viewModel"];

			// load CSRF token
			state.CsrfToken = viewModelToken["$csrfToken"].Value<string>();

			//			if (viewModelToken["$encryptedValues"] != null)
//			{
//				// load encrypted values
//				var encryptedValuesString = viewModelToken["$encryptedValues"].Value<string>();
//				viewModelConverter = new ViewModelJsonConverter(context.IsPostBack, mapper, JObject.Parse(viewModelProtector.Unprotect(encryptedValuesString, context)));
//			}
			var viewModelConverter = new ViewModelJsonConverter(true, mapper);

			// get validation path
//			context.ModelState.ValidationTargetPath = data.SelectToken("additionalData.validationTargetPath")?.Value<string>();

			// populate the ViewModel
			var serializer = CreateJsonSerializer();
			serializer.Converters.Add(viewModelConverter);
			try
			{
				viewModelConverter.Populate(viewModelToken.CreateReader(), serializer, state.LastSentViewModel);
			}
			catch (Exception ex)
			{
				throw new Exception($"Could not deserialize viewModel of type { state.LastSentViewModel.GetType().Name }. {GeneralViewModelRecommendations}", ex);
			}
		}
	}
}