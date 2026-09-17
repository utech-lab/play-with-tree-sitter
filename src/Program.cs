using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

using static TreeSitterNative;

/*
 テストファイル
	chatgptTest.cs testr.cs
	temp.html test.html virtualscroll.html
	test.php  test2.php test3.php
	test.css
	test.js test2.js test_adde.js email.js test_top.js add.js test_id_class.js
	test3.rs
 */

namespace TreeSitterTest
{
	internal class Program
	{
		static void Main(string[] args)
		{
			string libPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Libs");
			NativeMethods.SetDllDirectory(libPath);

			//string baseHtmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "html", "virtualscroll.html"); ;
			//if (File.Exists(baseHtmlPath) == false) {
			//	Console.Error.WriteLine($"'{baseHtmlPath}' not found");
			//	return;
			//}

			bool useSnippet = false;
			string sourcePath = null;
			string scmPath = null;

			foreach (var arg in args) {
				if (arg == "-s") {
					useSnippet = true;
				} else if (sourcePath == null) {
					sourcePath = arg;
				} else if (scmPath == null) {
					scmPath = arg;
				}
			}

			if (sourcePath == null) {
				Console.Error.WriteLine("引数には対象ファイルが必要です。");
				return;
			}
			//Stopwatch sw = new Stopwatch();
			//sw.Start();

			string fileId = Guid.NewGuid().ToString();

			string filename = sourcePath;
			string ext = Path.GetExtension(filename);
			var langDef = LanguageRegistry.GetByExtension(ext);
			if (langDef == null) {
				Console.WriteLine("Error: lang not found. " + filename);
				return;
			}

			byte[] buffer = MemoUtil.ReadAllByteText(filename);
			if (buffer == null) {
				return;
			}
			//buffer = System.IO.File.ReadAllBytes(file);		// 4kだと誤判定が結構出るんので全部でやる 
			//if (MemoUtil.IsText(buffer) == false)
			//	return;
			Encoding enc = MemoUtil.AutoDetectEncoding(buffer);
			string body = MemoUtil.GetStringWithoutBom(buffer, enc);
			body = MemoUtil.RemoveCR(body);     // \rを削除しておく。\r\nという改行コードはhtml出力時に邪魔になる可能性がある。htmlパーサは\r\nを\nに正規化する仕様 

			//Console.WriteLine($"RemoveCRまで:{sw.ElapsedMilliseconds / 1000.0:F2}");

			IntPtr parser = IntPtr.Zero;
			IntPtr tree = IntPtr.Zero;
			// List<TreeSitterSymbolInfo> symbol_list = new List<TreeSitterSymbolInfo>();
			CodeAnalysisResult result = new CodeAnalysisResult();
			string html = "";


			MemoryStream htmlMs = null;
			try {
				int lineCount = body.Count(c => c == '\n');
				int initialSize = Math.Max(100_000, lineCount * 100);

				htmlMs = new MemoryStream(initialSize);
				

				using (var writer = new StreamWriter(htmlMs, Encoding.UTF8, 4096, leaveOpen: true)) {
					if (useSnippet == false) {
						writer.Write(MemoUtil.VirtualScrollHtmlPart1);
						TreeSitterUtil.GenerateVirtualScrollHtml(writer, langDef, body, result);
						writer.Write(MemoUtil.VirtualScrollHtmlPart2);

						List<SymbolMatch> allList = new List<SymbolMatch>();
						List<CallInfo> allCallList = new List<CallInfo>();
						foreach (var root in result.Langs) {
							var resLangDef = LanguageRegistry.GetByName(root.ElementRoot.Language);
							if (result.Langs.Count > 1) {
								// ele_detail += $"<details style='margin-left: 15px;'><summary><b>[{root.ElementRoot.Language}]</b></span></summary>" + TreeSitterUtil.ToHtmlTree(root.ElementRoot) + "</details>";
								//writer.WriteLine($"<details style='margin-left: 15px;'><summary><b>[{root.ElementRoot.Language}]</b></span></summary>"  + TreeSitterUtil.RenderOutline(root.Root) + "</details>");
								writer.WriteLine($"<details style='margin-left: 15px;'><summary><span class=\"name definition-lang\" title=\"{root.ElementRoot.Language}\"><b>[{root.ElementRoot.Language}]</b></span></summary><div class=\"tree-children\">" + resLangDef.RenderOutline(resLangDef, root.Root) + "</div></details>");

							} else {
								// ele_detail += TreeSitterUtil.ToHtmlTree(root.ElementRoot);
								// LanguageRegistry
								//writer.WriteLine(LanguageRegistry.GetByName(root.ElementRoot.Language).RenderOutline(root.Root));
								writer.WriteLine(resLangDef.RenderOutline(resLangDef, root.Root));
								//writer.WriteLine(TreeSitterUtil.RenderOutline(root.Root));
							}
							if (root.Symbols != null) {
								allList.AddRange(root.Symbols);
							}
							List<CallInfo> callList = TreeSitterUtil.CollectCallInfos(root.Symbols, root.Root);
							if (callList != null && callList.Count > 0) {
								allCallList.AddRange(callList);
							}
						}
						var sortedCallList = allCallList
							.Where(x => !string.IsNullOrWhiteSpace(x.Symbol))
							.OrderBy(x => x.Symbol)
							.ThenBy(x => x.Node.LineNumber)
							.ToList();
						writer.Write($"<script>  thisFileId = '{fileId}'; allSymbolsData = " + JsonWrapper.SerializeSymbols(allList) + ";"
							+ "allCallData = " + JsonWrapper.SerializeCalls(sortedCallList) + ";</script>");
						writer.Write(MemoUtil.VirtualScrollHtmlPart3);
					} else {
						writer.Write(MemoUtil.SnippetlHtml);
						TreeSitterUtil.GenerateSnippetHtml(writer, langDef, body);
					}
					writer.Flush();
				}

				// AllData?.MemoInfo?.OrgFileName 
				htmlMs.Position = 0;
				using (var fs = File.Create("result.html")) {
					htmlMs.CopyTo(fs);
				}
				//htmlMs.Position = 0;


			} catch (Exception e) {
				Console.WriteLine(e.ToString());
			} finally {
				ts_tree_delete(tree);
				ts_parser_delete(parser);
			}

			if (result?.Langs != null) {
				string temp_html = MemoUtil.ElementsTemplateHtml;
				string ele_detail = "";

				foreach (var root in result.Langs) {

					if (result.Langs.Count > 1) {
						ele_detail += $"<details style='margin-left: 15px;'><summary><b>[{root.ElementRoot.Language}]</b></span></summary>" + TreeSitterUtil.ToHtmlTree(root.ElementRoot) + "</details>";
					} else {
						ele_detail += TreeSitterUtil.ToHtmlTree(root.ElementRoot);

					}
				}
				temp_html = temp_html.Replace("/*** replace detail tree by C# ***/", ele_detail);
				// temp_html = temp_html.Replace("/*** replace source code ***/", System.Web.HttpUtility.HtmlEncode(body));
				var sb = new StringBuilder();

				var lines = body.Replace("\r\n", "\n").Split('\n');

				for (int i = 0; i < lines.Length; i++) {
					sb.Append($"<div class=\"line\" data-line=\"{i + 1}\">");
					sb.Append($"<span class=\"ln\">{i + 1}</span>");
					sb.Append("<span class=\"code\">");
					sb.Append(System.Web.HttpUtility.HtmlEncode(lines[i]));
					sb.Append("</span>");
					sb.AppendLine("</div>");
				}

				temp_html = temp_html.Replace("/*** replace source code ***/", sb.ToString());

				using (var wr = new StreamWriter("elements.html")) {
					wr.Write(temp_html);
				}
			}
			if (result?.Langs != null) {
				string temp_html = MemoUtil.ElementsTemplateHtml;
				string ele_detail = "";

				foreach (var root in result.Langs) {
					if (result.Langs.Count > 1) {
						ele_detail += $"<details style='margin-left: 15px;'><summary><b>[{root.ElementRoot.Language}]</b></span></summary>" + TreeSitterUtil.ToHtmlTree(root.ElementRoot) + "</details>";
					} else {
						ele_detail += TreeSitterUtil.ToHtmlTree(root.Root);
					}
				}
				temp_html = temp_html.Replace("/*** replace detail tree by C# ***/", ele_detail);

				var sb = new StringBuilder();

				var lines = body.Replace("\r\n", "\n").Split('\n');

				for (int i = 0; i < lines.Length; i++) {
					sb.Append($"<div class=\"line\" data-line=\"{i + 1}\">");
					sb.Append($"<span class=\"ln\">{i + 1}</span>");
					sb.Append("<span class=\"code\">");
					sb.Append(System.Web.HttpUtility.HtmlEncode(lines[i]));
					sb.Append("</span>");
					sb.AppendLine("</div>");
				}


				temp_html = temp_html.Replace("/*** replace source code ***/", sb.ToString());
				using (var wr = new StreamWriter("symbols.html")) {
					wr.Write(temp_html);
				}
			}

			if (result?.Langs != null) {

				using (var wr = new StreamWriter("tags.txt")) {
					foreach (var root in result.Langs) {
						if (root.Symbols == null) {
							continue;
						}
						wr.WriteLine($"[[ {root.ElementRoot.Language} ]]");
						foreach (var item in root.Symbols) {
							wr.WriteLine("-----------------------------");
							wr.WriteLine($"{item.Name} @{item.CaptureType} {(item.IsPublic ? "P" : "C")} {item.Accessibility} {(item.IsStatic ? "static" : "")}  full:{item.FullQualifierName}");
							wr.WriteLine($"{item.Declaration}");
							wr.WriteLine($"range: {item.Node.StartByte} ～ {item.Node.EndByte} (len:{item.Node.EndByte - item.Node.StartByte}). lang: {item.Language}");
							wr.WriteLine(item.Node.Content);
						}
					}
				}
			}
			
			//if (symbol_list != null && symbol_list.Count > 0) {

			//	using (var writer = new StreamWriter("symbols.txt")) {
			//		foreach (var item in symbol_list) {
			//			writer.WriteLine($"{item.Name}\t{item.Type}\t{item.LineNumber}");
			//		}
			//	}
			//}

				//sw.Stop();
				//Console.WriteLine($"経過秒数:{sw.ElapsedMilliseconds / 1000.0:F2}");
			//	long bytes = GC.GetTotalMemory(forceFullCollection: false);
			//Console.WriteLine($"使用メモリ: {bytes / 1024.0 / 1024.0:F2} MB");
		}
	}
}
