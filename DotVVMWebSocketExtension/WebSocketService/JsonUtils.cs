﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace DotVVMWebSocketExtension.WebSocketService
{
//    public static class JsonUtils
//    {
//		public static JObject Diff(JObject source, JObject target, bool nullOnRemoved = false)
//		{
//			var diff = new JObject();
//			foreach (var item in target)
//			{
//				var sourceItem = source[item.Key];
//				if (sourceItem == null)
//				{
//					if (item.Value != null)
//					{
//						diff[item.Key] = item.Value;
//					}
//				}
//				else if (sourceItem.Type == JTokenType.Date)
//				{
//					if (item.Value.Type != JTokenType.String && item.Value.Type != JTokenType.Date)
//						diff[item.Key] = item.Value;
//					else
//					{
//						var sourceTime = sourceItem.ToObject<DateTime>();
//
//						DateTime targetTime;
//						if (item.Value.Type == JTokenType.Date)
//						{
//							targetTime = item.Value.ToObject<DateTime>();
//						}
//						else
//						{
//							var targetJson = $@"{{""Time"": ""{item.Value}""}}";
//							targetTime = JObject.Parse(targetJson)["Time"].ToObject<DateTime>();
//						}
//
//						if (!sourceTime.Equals(targetTime))
//						{
//							diff[item.Key] = item.Value;
//						}
//					}
//				}
//				else if (sourceItem.Type != item.Value.Type)
//				{
//					if (sourceItem.Type == JTokenType.Object || sourceItem.Type == JTokenType.Array
//						|| item.Value.Type == JTokenType.Object || item.Value.Type == JTokenType.Array
//						|| item.Value.ToString() != sourceItem.ToString())
//					{
//
//						diff[item.Key] = item.Value;
//					}
//				}
//				else if (sourceItem.Type == JTokenType.Object) // == item.Value.Type
//				{
//					var itemDiff = Diff((JObject)sourceItem, (JObject)item.Value, nullOnRemoved);
//					if (itemDiff.Count > 0)
//					{
//						diff[item.Key] = itemDiff;
//					}
//				}
//				else if (sourceItem.Type == JTokenType.Array)
//				{
//					var sourceArr = (JArray)sourceItem;
//					var subchanged = false;
//					var arrDiff = Diff(sourceArr, (JArray)item.Value, out subchanged, nullOnRemoved);
//					if (subchanged)
//					{
//						diff[item.Key] = arrDiff;
//					}
//				}
//				else if (!JToken.DeepEquals(sourceItem, item.Value))
//				{
//					diff[item.Key] = item.Value;
//				}
//			}
//
//			if (nullOnRemoved)
//			{
//				foreach (var item in source)
//				{
//					if (target[item.Key] == null) diff[item.Key] = JValue.CreateNull();
//				}
//			}
//
//			// remove abandoned $options
//			foreach (var item in Enumerable.ToArray<KeyValuePair<string, JToken>>(diff))
//			{
//				if (item.Key.EndsWith("$options", StringComparison.Ordinal))
//				{
//					if (diff[item.Key.Remove(item.Key.Length - "$options".Length)] == null)
//					{
//						diff.Remove(item.Key);
//					}
//				}
//			}
//			return diff;
//		}
//
//	    public static JArray Diff(JArray source, JArray target, out bool changed, bool nullOnRemoved = false)
//	    {
//		    changed = source.Count != target.Count;
//		    var diffs = new JToken[target.Count];
//		    var commonLen = Math.Min(diffs.Length, source.Count);
//		    for (int i = 0; i < commonLen; i++)
//		    {
//			    if (target[i].Type == JTokenType.Object && source[i].Type == JTokenType.Object)
//			    {
//				    diffs[i] = Diff((JObject)source[i], (JObject)target[i], nullOnRemoved);
//				    if (((JObject)diffs[i]).Count > 0) changed = true;
//			    }
//			    else if (target[i].Type == JTokenType.Array && source[i].Type == JTokenType.Array)
//			    {
//				    diffs[i] = Diff((JArray)source[i], (JArray)target[i], out var subchanged, nullOnRemoved);
//				    if (subchanged) changed = true;
//			    }
//			    else
//			    {
//				    diffs[i] = target[i];
//				    if (!JToken.DeepEquals(source[i], target[i]))
//					    changed = true;
//			    }
//		    }
//		    for (int i = commonLen; i < diffs.Length; i++)
//		    {
//			    diffs[i] = target[i];
//			    changed = true;
//		    }
//		    return new JArray(diffs);
//	    }
//
//
//	    public static JArray DiffArr(JArray source, JArray target, out bool changed, bool nullOnRemoved = false)
//	    {
//		    changed = true;
//		    return target;
//	    }
//	}
}
