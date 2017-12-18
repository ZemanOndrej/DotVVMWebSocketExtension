using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Security;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVMWebSocketExtension.WebSocketService;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DotVVMWebSocketExtension.Tests
{
	/// <summary>
	/// this class should test serializer for viewmodel 
	/// most of the code is used from DotVVMViewModelSerializer 
	/// this class is hard to test
	/// </summary>
	public class WebSocketViewModelSerializerTests
	{
		public Mock<IViewModelSerializationMapper> mapper { get; set; }
		public Mock<ViewModelState>ViewModelState { get; set; }
		public WebSocketViewModelSerializer serializer { get; set; }

		public WebSocketViewModelSerializerTests()
		{
			mapper = new Mock<IViewModelSerializationMapper>();
			ViewModelState = new Mock<ViewModelState>();
			serializer = new WebSocketViewModelSerializer(mapper.Object);
		}

		[Fact]
		public void TestSerializeViewModelWithDiff()
		{
			var received = Helpers.CreateRecieveJObject;
			var currentView = Helpers.CreateViewModelJObject;

			ViewModelState.Object.ChangedViewModelJson = currentView;
			ViewModelState.Object.LastSentViewModelJson = received;
			Assert.NotNull(ViewModelState.Object.ChangedViewModelJson["viewModel"]);

			var str = serializer.SerializeViewModel(ViewModelState.Object);
			Assert.Equal("{\"action\":\"successfulCommand\",\"viewModelDiff\":{\"Text\":\"Stage 3\"}}", str);
			Assert.Null(ViewModelState.Object.ChangedViewModelJson["viewModel"]);
			Assert.DoesNotContain("\"viewModel\"", str);
		}

	}

}