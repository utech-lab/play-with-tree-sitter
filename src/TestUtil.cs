using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public static class NativeMethods
{
	public const int LVM_FIRST = 0x1000;
	public const int LVM_SETGROUPINFO = LVM_FIRST + 147;

	public const int LVGF_STATE = 0x00000004;

	public const int LVGS_NORMAL = 0x00000000;
	public const int LVGS_COLLAPSED = 0x00000001;
	public const int LVGS_COLLAPSIBLE = 0x00000008;

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct LVGROUP
	{
		public int cbSize;
		public int mask;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string pszHeader;
		public int cchHeader;
		public IntPtr pszFooter;
		public int cchFooter;
		public int iGroupId;
		public int stateMask;
		public int state;
		public int uAlign;
	}

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref LVGROUP lParam);


	[StructLayout(LayoutKind.Sequential)]
	public struct INPUT
	{
		public int type;
		public MOUSEKEYBDHARDWAREINPUT mkhi;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct MOUSEKEYBDHARDWAREINPUT
	{
		[FieldOffset(0)] public KEYBDINPUT ki;
	}

	public struct KEYBDINPUT
	{
		public ushort wVk;
		public ushort wScan;
		public uint dwFlags;
		public uint time;
		public IntPtr dwExtraInfo;
	}

	[DllImport("user32.dll", SetLastError = true)]
	public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

	public const int INPUT_KEYBOARD = 1;
	public const uint KEYEVENTF_KEYUP = 0x0002;
	public const uint KEYEVENTF_UNICODE = 0x0004;


	[DllImport("user32.dll")]
	public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

	public const int TVM_SETEXTENDEDSTYLE = 0x1100 + 44;
	public const int TVS_EX_DOUBLEBUFFER = 0x0004;
	public const int TVS_EX_AUTOHSCROLL = 0x0020; // ついでに横スクロールも滑らかにする



	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern bool SetDllDirectory(string lpPathName);
}




public class MemoUtil
{

	static MemoUtil()
	{
		const string marker = "/*insert by C#*/";
		const string marker2 = "/*insert by C# 2*/";

		string html = VirtualScrollHtml; // 埋め込みリソースを読み込む

		// 1つ目のマーカーを探す
		int idx1 = html.IndexOf(marker, StringComparison.Ordinal);

		if (idx1 < 0) {
			// マーカーが無い場合は全部 Part1 に入れて、他は空
			VirtualScrollHtmlPart1 = html;
			VirtualScrollHtmlPart2 = "";
			VirtualScrollHtmlPart3 = "";
			return;
		}

		// Part1 はマーカーの手前まで
		VirtualScrollHtmlPart1 = html.Substring(0, idx1);

		// 残りの部分（マーカー以降）を一旦テンポラリに保持
		string remainder = html.Substring(idx1 + marker.Length);

		// 2つ目のマーカーを「残りの部分」から探す
		int idx2 = remainder.IndexOf(marker2, StringComparison.Ordinal);

		if (idx2 < 0) {
			// 2つ目のマーカーが無い場合は、残りがすべて Part2 になり、Part3 は空
			VirtualScrollHtmlPart2 = remainder;
			VirtualScrollHtmlPart3 = "";
			return;
		}

		// Part2 は 2つ目のマーカーの手前まで
		VirtualScrollHtmlPart2 = remainder.Substring(0, idx2);

		// Part3 は 2つ目のマーカーの後ろすべて
		VirtualScrollHtmlPart3 = remainder.Substring(idx2 + marker2.Length);
	}
	public static string VirtualScrollHtml {
		get {
			return LoadHtml("TreeSitterTest.virtualscroll.html");
		}
	}
	public static string VirtualScrollHtmlPart1 {
		get; private set;
	}
	public static string VirtualScrollHtmlPart2 {
		get; private set;
	}
	public static string VirtualScrollHtmlPart3 {
		get; private set;
	}
	public static string SnippetlHtml {
		get {
			return LoadHtml("TreeSitterTest.snippet.html");
		}
	}
	public static string ElementsTemplateHtml {
		get {
			return LoadHtml("TreeSitterTest.elements_template.html");
		}
	}

	public static string LoadHtml(string resourceName)
	{
		var asm = Assembly.GetExecutingAssembly();

		using (var stream = asm.GetManifestResourceStream(resourceName))
		using (var reader = new StreamReader(stream))
			return reader.ReadToEnd();
	}

	public static string GetStringWithoutBom(byte[] buffer, Encoding enc)
	{
		// 指定されたエンコーディングのBOM（プリアンブル）を取得
		byte[] bom = enc.GetPreamble();

		// bufferがBOMより長く、かつ先頭がBOMと一致するか確認
		bool hasBom = buffer.Length >= bom.Length &&
					  bom.Select((b, i) => buffer[i] == b).All(x => x);

		int index = hasBom ? bom.Length : 0;
		int count = buffer.Length - index;

		// BOM以降の範囲を指定して文字列に変換
		return enc.GetString(buffer, index, count);
	}
	public static string RemoveCR(string input)
	{
		var sb = new StringBuilder(input.Length);

		for (int i = 0; i < input.Length; i++) {
			char c = input[i];
			if (c != '\r')
				sb.Append(c);
		}

		return sb.ToString();
	}

	public static byte[] RemoveBom(byte[] buffer, Encoding enc)
	{
		byte[] bom = enc.GetPreamble();

		bool hasBom = buffer.Length >= bom.Length &&
			  bom.Select((b, i) => buffer[i] == b).All(x => x);

		int index = hasBom ? bom.Length : 0;
		int count = buffer.Length - index;
		byte[] result = new byte[count];
		Buffer.BlockCopy(buffer, index, result, 0, count);
		return result;
	}

	public static byte[] ReadAllByteText(string path, long MaxFileSize = 100 * 1024 * 1024)
	{
		const int ChunkSize = 4096;          // 読み込み単位(4KB)

		try {
			var fileInfo = new System.IO.FileInfo(path);

			// 1. サイズチェック
			if (fileInfo.Length > MaxFileSize || fileInfo.Length == 0) {
				return null;
			}

			long fileSize = fileInfo.Length;
			byte[] fullBuffer = new byte[fileSize];

			using (var fs = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite)) {
				int totalRead = 0;
				while (totalRead < fileSize) {
					int remaining = (int)(fileSize - totalRead);
					int countToRead = Math.Min(ChunkSize, remaining);

					// 2. 4KB分を fullBuffer に直接読み込む
					int readNow = fs.Read(fullBuffer, totalRead, countToRead);
					if (readNow <= 0)
						break;

					byte[] tempChunk = new byte[readNow];
					Buffer.BlockCopy(fullBuffer, totalRead, tempChunk, 0, readNow);

					if (IsText(tempChunk) == false) {
						return null; // バイナリ混入確定、即終了
					}

					totalRead += readNow;
				}

				return fullBuffer;
			}
		} catch {
			// ファイルロックやアクセス権限エラーなどのケア
			return null;
		}
	}
	public static bool IsText(byte[] data)
	{
		if (data == null || data.Length == 0)
			return true;

		int controlCharCount = 0;
		//int sampleSize = Math.Min(data.Length, 4096);
		int sampleSize = data.Length;

		for (int i = 0; i < sampleSize; i++) {
			byte b = data[i];

			// NULLバイト → ほぼバイナリ
			if (b == 0)
				return false;

			// 制御文字チェック（タブ、改行などはOK）
			if (b < 0x20 && b != 0x09 && b != 0x0A && b != 0x0D)
				controlCharCount++;
		}

		// 制御文字が多すぎるならバイナリ扱い
		if ((double)controlCharCount / sampleSize > 0.3)
			return false;

		// UTF-8として妥当かチェック
		try {
			var utf8 = new System.Text.UTF8Encoding(false, true);
			utf8.GetString(data);
			return true;
		} catch {
			// UTF-8じゃなくてもテキストの可能性はある（Shift_JISなど）
			return true;
		}
	}

	public static Encoding AutoDetectEncoding(byte[] bytes)
	{

		Encoding bom = DetectByBom(bytes);
		if (bom != null) {
			return bom;
		}

		// utfのdomありをdomなしエンコードで読み込むとゴミが出る 
		// utfのdomなしをdomありエンコードで読み込むとゴミは出ない。 
		var candidates = new[] {
				Encoding.UTF8,						// bomあり 
				// new UTF8Encoding(false),			// bomなし 
				Encoding.GetEncoding("shift-jis"),
				Encoding.GetEncoding("euc-jp")
			};
		Encoding bestEncoding = Encoding.UTF8; // デフォルト
		int minErrorCount = int.MaxValue;
		foreach (var enc in candidates) {
			try {
				string text = enc.GetString(bytes);
				// U+FFFD（置換文字）の個数をカウント
				int errorCount = text.Count(c => c == '\uFFFD');

				if (errorCount < minErrorCount) {
					minErrorCount = errorCount;
					bestEncoding = enc;
				}

				// エラーが0なら、それが一番自然とみなして確定でOK
				if (minErrorCount == 0)
					break;
			} catch {
			}
		}
		return bestEncoding;
	}
	public static Encoding DetectByBom(byte[] bytes)
	{
		if (bytes.Length >= 3 &&
			bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
			return Encoding.UTF8;

		if (bytes.Length >= 2) {
			if (bytes[0] == 0xFF && bytes[1] == 0xFE)
				return Encoding.Unicode;      // UTF-16LE
			if (bytes[0] == 0xFE && bytes[1] == 0xFF)
				return Encoding.BigEndianUnicode; // UTF-16BE
		}

		return null; // BOMなし
	}


	public static void JsonEscape(string value, StringBuilder sb)
	{
		if (string.IsNullOrEmpty(value))
			return;

		// 1文字ずつ高速チェック
		for (int i = 0; i < value.Length; i++) {
			char c = value[i];

			switch (c) {
				// JSONの基本エスケープ
				case '"':
					sb.Append("\\\"");
					break;
				case '\\':
					sb.Append("\\\\");
					break;
				case '\b':
					sb.Append("\\b");
					break;
				case '\f':
					sb.Append("\\f");
					break;
				case '\n':
					sb.Append("\\n");
					break;
				case '\r':
					sb.Append("\\r");
					break;
				case '\t':
					sb.Append("\\t");
					break;

				// HTML/Scriptタグの誤作動（XSSやブラウザのパースエラー）を防ぐための
				// 正しい JSON 準拠の Unicode エスケープ
				case '<':
					sb.Append("\\u003c");
					break;
				case '>':
					sb.Append("\\u003e");
					break;
				case '&':
					sb.Append("\\u0026");
					break;

				default:
					// コントロール文字（制御文字）の安全対策
					if (c < ' ') {
						sb.AppendFormat("\\u{0:x4}", (int)c);
					} else {
						sb.Append(c);
					}
					break;
			}
		}
	}




}