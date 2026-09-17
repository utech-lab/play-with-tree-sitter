using System;
using System.Collections.Generic;
using System.Linq;
//using System.Windows.Documents;
//using System.Windows.Navigation;
//using MemoView;



// 環境ごとに使用するライブラリ（名前空間）を綺麗に切り替える
#if NETCOREAPP
using System.Text.Json;
#else
using Newtonsoft.Json;
#endif

public static class JsonWrapper
{
	public static string SerializeJson(object value)
	{
		string tmp;

#if NETCOREAPP
    var options = new JsonSerializerOptions
    {
        WriteIndented = true
    };

    tmp = JsonSerializer.Serialize(value, options);
#else
		tmp = JsonConvert.SerializeObject(value, Formatting.Indented);
#endif

		return tmp.Replace("</script>", "\\u003C/script\\u003E");
	}
	private static object CreateSymbolsData(
		List<SymbolMatch> list,
		bool need_fileid,
		bool need_lang)
	{
		return list.Select(s => new
		{
			nm = s.Name,
			ct = s.CaptureType,
			sb = s.StartByte,
			eb = s.EndByte,
			fi = need_fileid ? s.FileId : null,
			ln = s.Node?.LineNumber ?? -1,
			lg = need_lang ? (int)s.Language : 0,
			dc = s.Declaration
		}).ToList();
	}

	public static string SerializeSymbols(
		List<SymbolMatch> list,
		bool need_fileid = false,
		bool need_lang = false)
	{
		if (list == null || list.Count == 0)
			return "[]";

		return SerializeJson(CreateSymbolsData(list, need_fileid, need_lang));
	}



	private static object CreateCallsData(
		List<CallInfo> list,
		bool need_fileid,
		bool need_lang)
	{
		return list.Select(s => new {
			nm = s.Symbol,
			sb = s.StartByte,
			eb = s.EndByte,
			fi = need_fileid ? s.FileId : null,
			//lineNumber = (s.Node != null) ? s.Node.LineNumber : -1,
			ln = s.Node?.LineNumber ?? -1,
			lg = need_lang ? (int)s.Language : 0,
			dc = s.UsageContext
		}).ToList();
	}
	public static string SerializeCalls(List<CallInfo> list, bool need_fileid = false, bool need_lang = false)
	{
		if (list == null || list.Count == 0)
			return "[]";

		return SerializeJson(CreateCallsData(list, need_fileid, need_lang));
	}
	public static string SerializeCallsForPost(
		string type,
		string rid,
		List<CallInfo> list,
		bool need_fileid = false,
		bool need_lang = false)
	{
		var obj = new {
			type,
			rid,
			results = CreateCallsData(list, need_fileid, need_lang)
		};

		return SerializeJson(obj);
	}


	public static string SerializeCalls_org(List<CallInfo> list, bool need_fileid = false, bool need_lang = false)
	{
		if (list == null || list.Count == 0) {
			return "[]";
		}
		// 1. 渡すデータを必要なプロパティだけに絞り込む（共通処理）
		var lightList = list.Select(s => new {
			nm = s.Symbol,
			sb = s.StartByte,
			eb = s.EndByte,
			fi = need_fileid ? s.FileId : null,
			//lineNumber = (s.Node != null) ? s.Node.LineNumber : -1,
			ln = s.Node?.LineNumber ?? -1,
			lg = need_lang ? (int)s.Language : 0,
			dc = s.UsageContext
		}).ToList();
		string tmp;
		// 2. それぞれの環境の「一番強くて安定しているパーサー」で処理
#if NETCOREAPP
        // .NET Core / .NET 8 以降：標準の超高速パーサー
		var options = new JsonSerializerOptions
		{
			WriteIndented = true
		};
		tmp = JsonSerializer.Serialize(lightList, options);
#else
		// .NET Framework 4.8：安心と信頼の Newtonsoft.Json
		tmp = JsonConvert.SerializeObject(lightList, Formatting.Indented);
		//return JsonConvert.SerializeObject(lightList);
#endif
		return tmp.Replace("</script>", "\\u003C/script\\u003E");

	}




	// const file = guidFileDic[guid]; 
	public static string SerializeGuidFileDic(Dictionary<string, string> dic)
	{
		if (dic == null || dic.Count == 0) {
			return "[]";
		}
		// 2. それぞれの環境の「一番強くて安定しているパーサー」で処理
#if NETCOREAPP
        // .NET Core / .NET 8 以降：標準の超高速パーサー
        return JsonSerializer.Serialize(dic);
#else
		// .NET Framework 4.8：安心と信頼の Newtonsoft.Json
		return JsonConvert.SerializeObject(dic);
#endif
	}
}


