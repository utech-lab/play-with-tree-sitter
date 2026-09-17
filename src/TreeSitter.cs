using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Windows.Markup;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

using static TreeSitterNative;
using static TreeSitterUtil;


[StructLayout(LayoutKind.Sequential)]
public struct TSPoint
{
	public uint row;
	public uint column;
}
[StructLayout(LayoutKind.Sequential)]
public struct TSNode
{
	public uint context0;
	public uint context1;
	public uint context2;
	public uint context3;
	public IntPtr id;
	public IntPtr tree;
}
[StructLayout(LayoutKind.Sequential)]
public struct TSTreeCursor
{
	public IntPtr tree;
	public IntPtr id;
	public uint context0;
	public uint context1;
	public uint context2;
}

[StructLayout(LayoutKind.Sequential)]
public struct TSRange
{
	public TSPoint start_point;
	public TSPoint end_point;
	public uint start_byte;
	public uint end_byte;
}

[StructLayout(LayoutKind.Sequential)]
public struct TSInputEdit
{
	public uint start_byte;
	public uint old_end_byte;
	public uint new_end_byte;
	public TSPoint start_point;
	public TSPoint old_end_point;
	public TSPoint new_end_point;
}

[StructLayout(LayoutKind.Sequential)]
public struct TSQueryCapture
{
	public TSNode node;
	public uint index;
}

[StructLayout(LayoutKind.Sequential)]
public struct TSQueryMatch
{
	public uint id;
	public ushort pattern_index;
	public ushort capture_count;
	public IntPtr captures;
}
public enum TSQueryPredicateStepType
{
	TSQueryPredicateStepTypeDone,
	TSQueryPredicateStepTypeCapture,
	TSQueryPredicateStepTypeString,
}
[StructLayout(LayoutKind.Sequential)]
public struct TSQueryPredicateStep
{
	public TSQueryPredicateStepType type;
	public uint value_id;
}

public enum TSInputEncoding
{
	TSInputEncodingUTF8,
	TSInputEncodingUTF16
}
public enum TSQuantifier
{
	TSQuantifierZero = 0,
	TSQuantifierZeroOrOne,
	TSQuantifierZeroOrMore,
	TSQuantifierOne,
	TSQuantifierOneOrMore,
}
public enum TSQueryError
{
	TSQueryErrorNone = 0,
	TSQueryErrorSyntax,
	TSQueryErrorNodeType,
	TSQueryErrorField,
	TSQueryErrorCapture,
	TSQueryErrorStructure,
	TSQueryErrorLanguage,
}
public enum TSSymbolType
{
	TSSymbolTypeRegular,
	TSSymbolTypeAnonymous,
	TSSymbolTypeAuxiliary,
}



public static class TreeSitterNative
{
	private const string CoreDll = "tree-sitter.dll";

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ts_parser_parse(
		IntPtr parser,
		IntPtr oldTree,
		IntPtr input);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ts_parser_parse_string(
		IntPtr parser,
		IntPtr oldTree,
		IntPtr input,
		uint length);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ts_parser_new();

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ts_parser_delete(IntPtr parser);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool ts_parser_set_language(IntPtr parser, IntPtr language);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ts_parser_language(IntPtr parser);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool ts_parser_set_included_ranges(IntPtr parser, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] TSRange[] ranges, uint length);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
	public static extern TSRange[] ts_parser_included_ranges(IntPtr parser, out uint length);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ts_parser_parse_string(IntPtr parser, IntPtr oldTree, byte[] input, uint length);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ts_parser_parse_string_encoding(IntPtr parser, IntPtr oldTree, byte[] input, uint length, TSInputEncoding encoding);
	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ts_parser_reset(IntPtr parser);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ts_parser_set_timeout_micros(IntPtr parser, ulong timeout);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern ulong ts_parser_timeout_micros(IntPtr parser);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ts_parser_set_cancellation_flag(IntPtr parser, ref IntPtr flag);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ts_parser_cancellation_flag(IntPtr parser);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ts_tree_copy(IntPtr tree);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ts_tree_delete(IntPtr tree);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern TSNode ts_tree_root_node(IntPtr tree);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern TSNode ts_tree_root_node_with_offset(IntPtr tree, uint offsetBytes, TSPoint offsetPoint);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ts_tree_language(IntPtr tree);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ts_tree_included_ranges(IntPtr tree, out uint length);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ts_tree_included_ranges_free(IntPtr ranges);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ts_tree_edit(IntPtr tree, ref TSInputEdit edit);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ts_tree_get_changed_ranges(IntPtr old_tree, IntPtr new_tree, out uint length);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ts_node_type(TSNode node);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern ushort ts_node_symbol(TSNode node);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern uint ts_node_start_byte(TSNode node);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern TSPoint ts_node_start_point(TSNode node);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	[SuppressUnmanagedCodeSecurity]
	public static extern uint ts_node_end_byte(TSNode node);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern TSPoint ts_node_end_point(TSNode node);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ts_node_string(TSNode node);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ts_node_string_free(IntPtr str);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool ts_node_is_null(TSNode node);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool ts_node_is_named(TSNode node);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool ts_node_is_missing(TSNode node);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool ts_node_is_extra(TSNode node);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool ts_node_has_changes(TSNode node);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool ts_node_has_error(TSNode node);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern TSNode ts_node_parent(TSNode node);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern TSNode ts_node_child(TSNode node, uint index);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ts_node_field_name_for_child(TSNode node, uint index);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern uint ts_node_child_count(TSNode node);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern TSNode ts_node_named_child(TSNode node, uint index);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern uint ts_node_named_child_count(TSNode node);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern TSNode ts_node_child_by_field_name(TSNode self, byte[] field_name, uint field_name_length);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern TSNode ts_node_child_by_field_id(TSNode self, ushort fieldId);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern TSNode ts_node_next_sibling(TSNode self);
	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern TSNode ts_node_prev_sibling(TSNode self);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern TSNode ts_node_next_named_sibling(TSNode self);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern TSNode ts_node_prev_named_sibling(TSNode self);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern TSNode ts_node_first_child_for_byte(TSNode self, uint byteOffset);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern TSNode ts_node_first_named_child_for_byte(TSNode self, uint byteOffset);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern TSNode ts_node_descendant_for_byte_range(TSNode self, uint startByte, uint endByte);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern TSNode ts_node_descendant_for_point_range(TSNode self, TSPoint startPoint, TSPoint endPoint);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern TSNode ts_node_named_descendant_for_byte_range(TSNode self, uint startByte, uint endByte);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern TSNode ts_node_named_descendant_for_point_range(TSNode self, TSPoint startPoint, TSPoint endPoint);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool ts_node_eq(TSNode node1, TSNode node2);


	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern TSTreeCursor ts_tree_cursor_new(TSNode node);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ts_tree_cursor_delete(ref TSTreeCursor cursor);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ts_tree_cursor_reset(ref TSTreeCursor cursor, TSNode node);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern TSNode ts_tree_cursor_current_node(ref TSTreeCursor cursor);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ts_tree_cursor_current_field_name(ref TSTreeCursor cursor);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern ushort ts_tree_cursor_current_field_id(ref TSTreeCursor cursor);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool ts_tree_cursor_goto_parent(ref TSTreeCursor cursor);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool ts_tree_cursor_goto_next_sibling(ref TSTreeCursor cursor);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool ts_tree_cursor_goto_first_child(ref TSTreeCursor cursor);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern long ts_tree_cursor_goto_first_child_for_byte(ref TSTreeCursor cursor, uint byteOffset);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern long ts_tree_cursor_goto_first_child_for_point(ref TSTreeCursor cursor, TSPoint point);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern TSTreeCursor ts_tree_cursor_copy(ref TSTreeCursor cursor);


	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ts_query_delete(IntPtr query);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern uint ts_query_pattern_count(IntPtr query);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern uint ts_query_capture_count(IntPtr query);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern uint ts_query_string_count(IntPtr query);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern uint ts_query_start_byte_for_pattern(IntPtr query, uint patternIndex);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ts_query_predicates_for_pattern(IntPtr query, uint patternIndex, out uint length);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool ts_query_is_pattern_rooted(IntPtr query, uint patternIndex);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool ts_query_is_pattern_non_local(IntPtr query, uint patternIndex);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool ts_query_is_pattern_guaranteed_at_step(IntPtr query, uint byteOffset);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ts_query_capture_name_for_id(IntPtr query, uint id, out uint length);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern TSQuantifier ts_query_capture_quantifier_for_id(IntPtr query, uint patternId, uint captureId);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ts_query_string_value_for_id(IntPtr query, uint id, out uint length);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ts_query_disable_capture(IntPtr query, byte[] captureName, uint captureNameLength);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ts_query_disable_pattern(IntPtr query, uint patternIndex);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ts_query_cursor_new();

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ts_query_cursor_delete(IntPtr cursor);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ts_query_cursor_exec(IntPtr cursor, IntPtr query, TSNode node);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool ts_query_cursor_did_exceed_match_limit(IntPtr cursor);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern uint ts_query_cursor_match_limit(IntPtr cursor);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ts_query_cursor_set_match_limit(IntPtr cursor, uint limit);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ts_query_cursor_set_byte_range(IntPtr cursor, uint start_byte, uint end_byte);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ts_query_cursor_set_point_range(IntPtr cursor, TSPoint start_point, TSPoint end_point);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool ts_query_cursor_next_match(IntPtr cursor, out TSQueryMatch match);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ts_query_cursor_remove_match(IntPtr cursor, uint id);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool ts_query_cursor_next_capture(IntPtr cursor, out TSQueryMatch match, out uint capture_index);


	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ts_query_new(IntPtr language, byte[] source, uint source_len, out uint error_offset, out TSQueryError error_type);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern uint ts_language_symbol_count(IntPtr language);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ts_language_symbol_name(IntPtr language, ushort symbol);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern ushort ts_language_symbol_for_name(IntPtr language, byte[] str, uint length, bool is_named);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern uint ts_language_field_count(IntPtr language);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ts_language_field_name_for_id(IntPtr language, ushort fieldId);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern ushort ts_language_field_id_for_name(IntPtr language, byte[] str, uint length);

	[DllImport(CoreDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern TSSymbolType ts_language_symbol_type(IntPtr language, ushort symbol);




	[DllImport("tree-sitter-bash.dll", EntryPoint = "tree_sitter_bash")]
	public static extern IntPtr tree_sitter_bash();

	[DllImport("tree-sitter-c-sharp.dll", EntryPoint = "tree_sitter_c_sharp")]
	public static extern IntPtr tree_sitter_c_sharp();

	[DllImport("tree-sitter-c.dll", EntryPoint = "tree_sitter_c")]
	public static extern IntPtr tree_sitter_c();

	[DllImport("tree-sitter-cpp.dll", EntryPoint = "tree_sitter_cpp")]
	public static extern IntPtr tree_sitter_cpp();

	[DllImport("tree-sitter-css.dll", EntryPoint = "tree_sitter_css")]
	public static extern IntPtr tree_sitter_css();

	[DllImport("tree-sitter-go.dll", EntryPoint = "tree_sitter_go")]
	public static extern IntPtr tree_sitter_go();

	[DllImport("tree-sitter-html.dll", EntryPoint = "tree_sitter_html")]
	public static extern IntPtr tree_sitter_html();

	[DllImport("tree-sitter-java.dll", EntryPoint = "tree_sitter_java")]
	public static extern IntPtr tree_sitter_java();

	[DllImport("tree-sitter-javascript.dll", EntryPoint = "tree_sitter_javascript")]
	public static extern IntPtr tree_sitter_javascript();

	[DllImport("tree-sitter-php.dll", EntryPoint = "tree_sitter_php")]
	public static extern IntPtr tree_sitter_php();

	[DllImport("tree-sitter-python.dll", EntryPoint = "tree_sitter_python")]
	public static extern IntPtr tree_sitter_python();

	[DllImport("tree-sitter-ruby.dll", EntryPoint = "tree_sitter_ruby")]
	public static extern IntPtr tree_sitter_ruby();

	[DllImport("tree-sitter-rust.dll", EntryPoint = "tree_sitter_rust")]
	public static extern IntPtr tree_sitter_rust();

	[DllImport("tree-sitter-swift.dll", EntryPoint = "tree_sitter_swift")]
	public static extern IntPtr tree_sitter_swift();

	[DllImport("tree-sitter-typescript.dll", EntryPoint = "tree_sitter_typescript")]
	public static extern IntPtr tree_sitter_typescript();

	[DllImport("tree-sitter-tsx.dll", EntryPoint = "tree_sitter_tsx")]
	public static extern IntPtr tree_sitter_tsx();
}
public class LanguageDefinition
{
	public LanguageId Id {
		get;
	}
	public string PrimaryName {
		get;
	}
	public string[] Aliases {
		get;
	}
	public string[] Extensions {
		get;
	}
	public Func<IntPtr> GetLanguage {
		get;
	}

	public Func<LanguageDefinition, CodeAnalysisLangResult, List<SymbolMatch>> Parse {
		get;
	}
	public Func<LanguageDefinition, SymbolMatch, string> RenderOutline {
		get;
	}
	public bool RenderCallsFirst {
		get;
	}

	public LanguageDefinition(
		LanguageId id,
		string primaryName, string[] aliases, string[] extensions, Func<IntPtr> getLanguage, 
		Func<LanguageDefinition, CodeAnalysisLangResult, List<SymbolMatch>> parse, Func<LanguageDefinition, SymbolMatch, string> renderOutline, bool renderCallsFirst)
	{
		Id = id;
		PrimaryName = primaryName;
		Aliases = aliases;
		Extensions = extensions;
		GetLanguage = getLanguage;
		Parse = parse;
		if (renderOutline == null) {
			RenderOutline = TreeSitterUtil.RenderOutline;
		} else {
			RenderOutline = renderOutline;
		}
		RenderCallsFirst = renderCallsFirst;
	}
}
public enum LanguageId
{
	Non = 0,
	Bash = 1,
	CSharp = 2,
	C = 3,
	Cpp = 4,
	Css = 5,
	Go = 6,
	Html = 7,
	Java = 8,
	JavaScript = 9,
	Php = 10,
	Python = 11,
	Ruby = 12,
	Rust = 13,
	TypeScript = 14,
	Tsx = 15,
	Swift = 16,

	// 末尾に追加すること。 

	
}
public static class EnumExtensions
{
	// C#のenumをjs用のオブジェクト生成の文字列に変換する 
	// js内で if (symbol.lg === LanguageId.JavaScript) { ...  のように使用できるようになる 
	public static string GetJsEnumString<T>() where T : Enum
	{
		var type = typeof(T);
		var sb = new StringBuilder();

		sb.Append("const ");
		sb.Append(type.Name);
		sb.Append(" = Object.freeze({");

		var values = Enum.GetValues(type);

		for (int i = 0; i < values.Length; i++) {
			var value = values.GetValue(i);
			var name = Enum.GetName(type, value);

			if (i > 0)
				sb.Append(",");

			sb.Append(name);
			sb.Append(":");
			sb.Append(Convert.ToInt32(value));
		}

		sb.Append("});");

		return sb.ToString();
	}
}



public static class LanguageRegistry
{
	public static readonly List<LanguageDefinition> Languages = new List<LanguageDefinition>
	{
		new LanguageDefinition(LanguageId.Bash,		"bash",      new[] { "sh", "shell" },        new[] { ".sh", ".bash" },   TreeSitterNative.tree_sitter_bash, null, null, false),
		new LanguageDefinition(LanguageId.CSharp,	"c-sharp",        new[] { "cs", "csharp", "c#", "c-sharp" },  new[] { ".cs" },            TreeSitterNative.tree_sitter_c_sharp, TreeSitterUtil.CSharpParse, null, false),
		new LanguageDefinition(LanguageId.C,		"c",         new[] { "c" },                  new[] { ".c", ".h" },       TreeSitterNative.tree_sitter_c, null, null, false),
		new LanguageDefinition(LanguageId.Cpp,		"cpp",       new[] { "c++", "cc", "cpp" },   new[] { ".cpp", ".hpp", ".cc", ".cxx" }, TreeSitterNative.tree_sitter_cpp, null, null, false),
		new LanguageDefinition(LanguageId.Css,		"css",       new[] { "css" },                new[] { ".css" },           TreeSitterNative.tree_sitter_css, TreeSitterUtil.CssParse, null, false),
		new LanguageDefinition(LanguageId.Go,		"go",        new[] { "golang" },             new[] { ".go" },            TreeSitterNative.tree_sitter_go, null, null, false),
		new LanguageDefinition(LanguageId.Html,		"html",      new[] { "html", "htm" },        new[] { ".html", ".htm" },  TreeSitterNative.tree_sitter_html, TreeSitterUtil.HtmlParse, null, true),
		new LanguageDefinition(LanguageId.Java,		"java",      new[] { "java" },               new[] { ".java" },          TreeSitterNative.tree_sitter_java, null, null, false),
		new LanguageDefinition(LanguageId.JavaScript, "javascript", new[] { "js", "node" },        new[] { ".js", ".mjs", ".cjs" }, TreeSitterNative.tree_sitter_javascript, TreeSitterUtil.JavascriptParse, null, false),
		new LanguageDefinition(LanguageId.Php,		"php",    new[] { "php" },					new[] { ".php", ".phtml", ".php3", ".php4", ".php5" },            TreeSitterNative.tree_sitter_php, null, null, false),
		new LanguageDefinition(LanguageId.Python,	"python",    new[] { "py" },                 new[] { ".py" },            TreeSitterNative.tree_sitter_python, null, null, false),
		new LanguageDefinition(LanguageId.Ruby,		"ruby",      new[] { "rb", "ruby" },         new[] { ".rb" },            TreeSitterNative.tree_sitter_ruby, null, null, false),
		new LanguageDefinition(LanguageId.Rust,		"rust",      new[] { "rs" },                 new[] { ".rs" },            TreeSitterNative.tree_sitter_rust, null, null, false),
		//new LanguageDefinition(LanguageId.Swift, "swift",      new[] { "swift" },             new[] { ".swift" },         TreeSitterNative.tree_sitter_swift, null, null, false),
		new LanguageDefinition(LanguageId.TypeScript, "typescript",      new[] { "typescript", "ts" },   new[] { ".ts" },      TreeSitterNative.tree_sitter_typescript, null, null, false),
		new LanguageDefinition(LanguageId.Tsx,		"tsx",      new[] { "tsx" },					new[] { ".tsx" },			TreeSitterNative.tree_sitter_tsx, null, null, false),

//  
	};

	public static LanguageDefinition GetByExtension(string extension)
	{
		if (string.IsNullOrEmpty(extension))
			return null;
		string ext = extension.StartsWith(".") ? extension.ToLower() : "." + extension.ToLower();
		return Languages.FirstOrDefault(l => l.Extensions.Contains(ext));
	}
	public static LanguageDefinition GetByName(string name)
	{
		var n = name.ToLower();
		return Languages.FirstOrDefault(l => l.PrimaryName == n || l.Aliases.Contains(n));
	}

}


public class CodeAnalysisResult
{
	public string FileId {
		get;
	} = Guid.NewGuid().ToString();
	public CodeAnalysisResult(string fileId = null)
	{
		if (!string.IsNullOrEmpty(fileId)) {
			FileId = fileId;
		}
	}

	public List<CodeAnalysisLangResult> Langs = new List<CodeAnalysisLangResult>();
}
public class CodeAnalysisLangResult
{
	public TreeSitterElement ElementRoot { get; set; } = null;
	public SymbolMatch Root { get; set; } = new SymbolMatch { CaptureType = "root" };
	public List<SymbolMatch> Symbols { get; set; } = null;
}




public class TreeSitterSymbolInfo
{
	public string Name;
	public string Type; // Class, Method, etc.
	public int LineNumber;
}


public class TreeSitterUtil
{
	public static string GetHighlightsScmPath(LanguageDefinition langDef)
	{
		return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tree-sitter", langDef.PrimaryName + "_highlights.scm");
	}
	public static string GetInjectionsScmPath(LanguageDefinition langDef)
	{
		return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tree-sitter", langDef.PrimaryName + "_injections.scm");
	}

	public static string GenerateSnippetHtml(string lang, string body,  bool isInj = false)
	{
		string html = "";
		using (var ms = new MemoryStream()) {
			using (var writer = new StreamWriter(ms, Encoding.UTF8, 4096, leaveOpen: true)) {
				GenerateSnippetHtml(writer, lang, body);
				writer.Flush();
			}
			ms.Position = 0;
			using (var reader = new StreamReader(ms, Encoding.UTF8)) {
				html = reader.ReadToEnd();
			}
		}
		return html;
	}
	public static bool GenerateSnippetHtml(StreamWriter writer, string lang, string body, bool isInj = false)
	{
		var langDef = LanguageRegistry.GetByExtension(lang);
		if (langDef == null) {
			langDef = LanguageRegistry.GetByName(lang);
		}
		if (langDef == null) {
			return GenarateSnippetNoLangHtml(writer, body);
		}
		return GenerateSnippetHtml(writer, langDef, body,  isInj);
	}
	public static bool GenarateSnippetNoLangHtml(StreamWriter writer, string body)
	{
		body = body.Replace("\r", "");
		string[] lines = body.Split('\n');
		for (int i = 0; i < lines.Length; i++) {
			writer.Write($"<div class='ts-cl'><div class='ts-ln'>{i + 1}</div><div class='code-content'>{SafeHtmlEscape(lines[i])}\n</div></div>");
		}
		return true;
	}

	public static bool GenerateSnippetHtml(StreamWriter writer, LanguageDefinition langDef, string body, bool isInj = false)
	{
		if (langDef == null) {
			return false;
		}
		IntPtr parser = IntPtr.Zero;
		IntPtr tree = IntPtr.Zero;
		try {
			parser = ts_parser_new();
			ts_parser_set_language(parser, langDef.GetLanguage());
			byte[] utf8Body = Encoding.UTF8.GetBytes(body);
			tree = ts_parser_parse_string(parser, IntPtr.Zero, utf8Body, (uint)utf8Body.Length);
			var rootNode = ts_tree_root_node(tree);
			GenerateSnippetHtml(writer, langDef.GetLanguage(), rootNode, utf8Body,
				GetHighlightsScmPath(langDef),
				GetInjectionsScmPath(langDef),
				isInj
				);
		} catch (Exception e) {
			Console.WriteLine(e.ToString());
		} finally {
			ts_tree_delete(tree);
			ts_parser_delete(parser);
		}

		return true;
	}
	public static bool GenerateSnippetHtml(StreamWriter writer, IntPtr language, TSNode rootNode, byte[] utf8Body, string scmPath, string injPath,  bool isInj = false)
	{
		if (!File.Exists(scmPath))
			return false;

		var ranges = new List<HighlightRange>();
		var lineStarts = BuildLineIndex(utf8Body);

		if (File.Exists(injPath)) {
			var inj_list = ProcessInjQuery(language, rootNode, utf8Body, injPath);
			if (inj_list != null) {
				ranges.AddRange(inj_list);
			}
		}

		var tmp_ranges = GetRangeList(language, rootNode, scmPath);
		if (tmp_ranges == null) {
			return false;
		}
		ranges.AddRange(tmp_ranges);


		// コメント等で 範囲に\rもしくは\nが挿入されていたので、それを削除した。
		// これをしないと*.cの//コメントがあると次行が空行になる。
		for (int i = 0; i < ranges.Count; i++) {
			var range = ranges[i];
			if (range.CaptureName.StartsWith("comment")) {
				while (range.EndByte > range.StartByte &&
					   (utf8Body[range.EndByte - 1] == (byte)'\n' || utf8Body[range.EndByte - 1] == (byte)'\r')) {
					range.EndByte--;
				}
				ranges[i] = range;
			}
		}

		var sortedRanges = FlattenAndSplitRanges(ranges);

		StringBuilder sb = new StringBuilder();
		int currentLine = 1;
		int currentPos = 0;
		AppendRowStart(sb, currentLine, isInj);

		foreach (var range in sortedRanges) {
			// 1. ハイライト間のテキストを処理（内部で改行があれば行を割る）
			if (range.StartByte > currentPos) {
				AppendTextWithLineBreaking(writer, sb, "", utf8Body, currentPos, range.StartByte, ref currentLine, isInj);
			}

			string cssClass = range.CaptureName.Replace(".", "-");
			string tag = $"<span class='ts-{cssClass}' title='{cssClass}'>";
			// writer.Write(tag);
			sb.Append(tag);
			// 3. ハイライトされるテキストを処理（複数行にまたがるコメント等もこれで安全に割れる）
			AppendTextWithLineBreaking(writer, sb, tag, utf8Body, range.StartByte, range.EndByte, ref currentLine, isInj);

			// 4. ハイライト終了タグ
			// writer.Write("</span>");
			sb.Append("</span>");
			currentPos = Math.Max(currentPos, range.EndByte);
		}
		// 5. 最後に残った末尾のテキストを処理
		if (currentPos < utf8Body.Length) {
			AppendTextWithLineBreaking(writer, sb, "", utf8Body, currentPos, utf8Body.Length, ref currentLine, isInj);
		}
		AppendRowEnd(writer, sb, isInj, currentLine == 1);



		return true;
	}
	public static void AppendInjHtml(StreamWriter writer, StringBuilder sb, string injStr, ref int lineCounter, bool isInj)
	{
		AppendRowEnd(writer, sb, isInj, lineCounter == 1);
		string html = ReplaceAllCodeContentDivs(injStr, ref lineCounter);
		writer.Write(html);

		lineCounter++;
		AppendRowStart(sb, lineCounter, isInj);
	}

	public static void AppendTextWithLineBreaking(StreamWriter writer, StringBuilder sb,
		string tag, byte[] utf8Bytes, int start, int end, ref int lineCounter, bool isInj)
	{
		int segmentStart = start;

		for (int i = start; i < end; i++) {
			// UTF-8の改行コード（0x0A = '\n'）をチェック
			if (utf8Bytes[i] == 0x0A) {
				// 改行の手前までの文字列をエスケープして出力
				if (i > segmentStart) {
					string html = Substring(utf8Bytes, segmentStart, i);
					sb.Append(SafeHtmlEscape(html));
					//writer.Write(SafeHtmlEscape(utf8Bytes, segmentStart, i));
				}
				if (!string.IsNullOrEmpty(tag)) {
					//writer.Write("</span>");
					sb.Append("</span>");
				}

				// 現在の行を閉じて、次の行を開く！
				AppendRowEnd(writer, sb, isInj, lineCounter == 1);
				lineCounter++;
				AppendRowStart(sb, lineCounter, isInj);
				if (!string.IsNullOrEmpty(tag)) {
					// writer.Write(tag);
					sb.Append(tag);
				}
				segmentStart = i + 1; // 次のセグメントは改行の次の文字から
			}
		}

		// 残りのテキストがあれば出力
		if (end > segmentStart) {
			string html = Substring(utf8Bytes, segmentStart, end);
			sb.Append(SafeHtmlEscape(html));
		}
	}

	public static void AppendRowStart(StringBuilder sb, int lineNum, bool isInj)
	{
		if (isInj) {
			sb.Append("<div class='code-content'>");
		} else {
			sb.Append("<div class='ts-cl'><div class='ts-ln'>");
			sb.Append(lineNum);
			sb.Append("</div><div class='code-content'>");
		}
	}

	public static void AppendRowEnd(StreamWriter writer, StringBuilder sb, bool isInj,  bool firstLine)
	{
		sb.Append("</div></div>");
		writer.Write(sb.ToString());
		sb.Clear();
	}

	public static string ReplaceAllCodeContentDivs(string html, ref int lineNum)
	{
		const string target = "<div class='code-content'>";
		int pos = 0;

		while (true) {
			pos = html.IndexOf(target, pos);
			if (pos < 0)
				break;

			string replacement =
				$"<div class='ts-cl ts-ju'><div class='ts-ln'>{++lineNum}</div><div class='code-content'>";

			html = html.Substring(0, pos)
				 + replacement
				 + html.Substring(pos + target.Length);

			pos += replacement.Length; // 次の検索開始位置を進める
		}

		return html;
	}




	public static bool GenerateVirtualScrollHtml(StreamWriter writer, string lang, string body, CodeAnalysisResult result)
	{
		var langDef = LanguageRegistry.GetByExtension(lang);
		if (langDef == null) {
			langDef = LanguageRegistry.GetByName(lang);
		}
		if (langDef == null) {
			return false;
		}
		return GenerateVirtualScrollHtml(writer, langDef, body, result);
	}

	public static bool GenerateVirtualScrollHtml(StreamWriter writer, LanguageDefinition langDef, string body, CodeAnalysisResult result)
	{
		if (langDef == null) {
			return GenerateVirtualScrollNoLangHtml(writer, body);
		}
		IntPtr parser = IntPtr.Zero;
		IntPtr tree = IntPtr.Zero;
		try {
			//Stopwatch sw = new Stopwatch();
			parser = ts_parser_new();


			var lang = langDef.GetLanguage();
			//sw.Start();
			bool set_ok = ts_parser_set_language(parser, lang);
			if (set_ok == false) {
				return false;
			}
			//sw.Stop();
			//Console.WriteLine($"ts_parser_set_language:{sw.ElapsedMilliseconds / 1000.0:F2}");

			//sw.Restart();
			byte[] utf8Body = Encoding.UTF8.GetBytes(body);
			//sw.Stop();
			//Console.WriteLine($"Encoding.UTF8.GetBytes:{sw.ElapsedMilliseconds / 1000.0:F2}");

			//sw.Restart();
			tree = ts_parser_parse_string(parser, IntPtr.Zero, utf8Body, (uint)utf8Body.Length);
			//sw.Stop();
			//Console.WriteLine($"ts_parser_parse_string:{sw.ElapsedMilliseconds / 1000.0:F2}");

			//sw.Restart();
			var rootNode = ts_tree_root_node(tree);
			//sw.Stop();
			//Console.WriteLine($"ts_tree_root_node:{sw.ElapsedMilliseconds / 1000.0:F2}");

			GenerateVirtualScrollHtml(writer, langDef, rootNode, body, utf8Body, result);




			bool need_debug_data = true;
			if (need_debug_data) {

				List<BlockRange> range_list = new List<BlockRange>();
				FindAllBlocks(rootNode, utf8Body, range_list);
				using (var wr = new StreamWriter("blocks.txt")) {
					for (int i = 0; i < range_list.Count; i++) {
						wr.WriteLine($"------ {i} ------");
						wr.WriteLine($"length: {range_list[i].EndByte - range_list[i].StartByte}. range: {range_list[i].StartByte} ～ {range_list[i].EndByte}");
						wr.WriteLine(range_list[i].Content);
					}
				}
				List<TreeSitterElementFlat> ele_list = new List<TreeSitterElementFlat>();
				FindAllFlatElement(rootNode, utf8Body, ele_list);
				using (var wr = new StreamWriter("elements.txt")) {
					for (int i = 0; i < ele_list.Count; i++) {
						wr.WriteLine($"------ {i} ------");
						wr.WriteLine($"type: {ele_list[i].TypeName}");
						wr.WriteLine($"length: {ele_list[i].EndByte - ele_list[i].StartByte}. range: {ele_list[i].StartByte} ～ {ele_list[i].EndByte}");
						wr.WriteLine(ele_list[i].Content);
					}
				}


			}
		} catch (Exception e) {
			Console.WriteLine(e.ToString());
		} finally {
			ts_tree_delete(tree);
			ts_parser_delete(parser);
		}

		return true;
	}

	public static bool GenerateVirtualScrollNoLangHtml(StreamWriter writer, string body)
	{
		byte[] utf8Body = Encoding.UTF8.GetBytes(body);

		var lineStartOffsets = BuildLineIndex(utf8Body);
		List<HighlightRange> sortedRanges = new List<HighlightRange>();
		var map = BuildUtf8ToUtf16Map(body);
		ChangeUtf8to16(map, lineStartOffsets, sortedRanges);
		var lineRangeBoundaries = new List<int>();

		int rangeCount = sortedRanges.Count;
		int currentRangeStartIdx = 0;



		for (int i = 0; i < lineStartOffsets.Length; i++) {
			long lineStart = lineStartOffsets[i];
			long lineEnd = (i + 1 < lineStartOffsets.Length) ? lineStartOffsets[i + 1] : Encoding.Unicode.GetByteCount(body);

			// 1. この行の開始位置より前に完全に終わっているハイライトを読み飛ばす
			while (currentRangeStartIdx < rangeCount && sortedRanges[currentRangeStartIdx].EndByte <= lineStart) {
				currentRangeStartIdx++;
			}

			// 2. この行の「開始インデックス」を記録
			int startIdx = currentRangeStartIdx;
			int endIdx = currentRangeStartIdx;

			// 3. この行に関係のあるハイライトがどこまで続くか（終了インデックス）を調べる
			for (int k = currentRangeStartIdx; k < rangeCount; k++) {
				var range = sortedRanges[k];

				// この行の終わり以降にあるハイライトに到達したらストップ
				if (range.StartByte >= lineEnd) {
					break;
				}

				// 行と交差している場合、終了位置を更新していく
				if (range.EndByte > lineStart && range.StartByte < lineEnd) {
					endIdx = k + 1; // 該当するインデックスの「次の位置」を終了境界にする（for文の条件で未満にするため）
				}
			}

			// もし1件も交差しなかった場合は startIdx と endIdx が同じ値になり、JS側で件数0と判定できる
			lineRangeBoundaries.Add(startIdx);
			lineRangeBoundaries.Add(endIdx);
		}

		var parts = body.Split(new[] { "</script>" }, StringSplitOptions.None);
		for (int i = 0; i < parts.Length; i++) {
			writer.WriteLine(
				$"<script class=\"rawdata\" type=\"text/plain\">{SafeHtmlEscape(parts[i])}</script>");
		}

		writer.WriteLine();
		writer.WriteLine("<script>");
		writer.WriteLine(EnumExtensions.GetJsEnumString<LanguageId>());

		//writer.WriteLine($"const lineStartOffsets = [{string.Join(",", lineStartOffsets)}];");
		var values = lineStartOffsets.ToArray();
		writer.Write("const lineStartOffsets = [");
		for (int i = 0; i < values.Length; i++) {
			if (i > 0)
				writer.Write(",");
			if (i > 0 && i % 50 == 0)
				writer.WriteLine();
			writer.Write(values[i]);
		}
		writer.WriteLine("];");

		writer.WriteLine($"const highlightRanges = [];");
		writer.WriteLine("const captureNames = {};");

		// writer.WriteLine($"const utf8Index = {utfIndexSb.ToString()};");


		//writer.WriteLine($"const lineRangeBoundaries = [{string.Join(",", lineRangeBoundaries)}];");
		values = lineRangeBoundaries.ToArray();
		writer.Write("const lineRangeBoundaries = [");
		for (int i = 0; i < values.Length; i++) {
			if (i > 0)
				writer.Write(",");
			if (i > 0 && i % 50 == 0)
				writer.WriteLine();
			writer.Write(values[i]);
		}
		writer.WriteLine("];");
		return true;
	}
	public static bool GenerateVirtualScrollHtml(StreamWriter writer, LanguageDefinition langDef, TSNode rootNode, string body, byte[] utf8Body, CodeAnalysisResult result)
	{
		IntPtr language = langDef.GetLanguage();
		//Stopwatch sw = new Stopwatch();
		//sw.Start();
		var ranges = GetRangeList(rootNode, langDef, body);
		if (ranges == null) {
			return false;
		}
		//sw.Stop();
		//Console.WriteLine($"GetRangeList:{sw.ElapsedMilliseconds / 1000.0:F2}");
		//sw.Restart();
		var sortedRanges = FlattenAndSplitRanges(ranges); // ※開始バイト順にソートされている前提
		//sw.Stop();
		//Console.WriteLine($"FlattenAndSplitRanges:{sw.ElapsedMilliseconds / 1000.0:F2}");
		//sw.Restart();
		var lineStartOffsets = BuildLineIndex(utf8Body);
		//sw.Stop();
		//Console.WriteLine($"BuildLineIndex:{sw.ElapsedMilliseconds / 1000.0:F2}");
		var nameMap = new Dictionary<string, int>();
		//sw.Restart();
		var map = BuildUtf8ToUtf16Map(body);
		ChangeUtf8to16(map, lineStartOffsets, sortedRanges);
		//sw.Stop();
		//Console.WriteLine($"ChangeUtf8to16:{sw.ElapsedMilliseconds / 1000.0:F2}");

		//sw.Restart();
		// 1. 通常の highlightRanges と captureNames の出力用データの組み立て
		StringBuilder rangeSb = new StringBuilder();
		for (int i = 0; i < sortedRanges.Count; i++) {
			var range = sortedRanges[i];
			if (nameMap.TryGetValue(range.CaptureName, out var cindex) == false) {
				cindex = nameMap.Count + 1;
				nameMap.Add(range.CaptureName, cindex);
			}
			if (i != 0) {
				rangeSb.Append(',');
				if ((i % 10) == 0) {
					rangeSb.Append('\n');
				}
			}
			rangeSb.Append(range.StartByte)
				   .Append(',')
				   .Append(range.EndByte)
				   .Append(',')
				   .Append(cindex);
		}

		var mapSb = new StringBuilder();
		mapSb.Append('{');
		bool isFirst = true;
		foreach (var kvp in nameMap) {
			if (!isFirst)
				mapSb.Append(',');
			isFirst = false;
			mapSb.Append(kvp.Value).Append(':').Append('\"').Append(kvp.Key).Append('\"');
		}
		mapSb.Append('}');

		// ★★★ 【決定版】各行の「ハイライト開始位置・終了位置」のペアを1つの配列にする ★★★
		var lineRangeBoundaries = new List<int>();

		int rangeCount = sortedRanges.Count;
		int currentRangeStartIdx = 0;

		

		for (int i = 0; i < lineStartOffsets.Length; i++) {
			long lineStart = lineStartOffsets[i];
			long lineEnd = (i + 1 < lineStartOffsets.Length) ? lineStartOffsets[i + 1] : Encoding.Unicode.GetByteCount(body);

			// 1. この行の開始位置より前に完全に終わっているハイライトを読み飛ばす
			while (currentRangeStartIdx < rangeCount && sortedRanges[currentRangeStartIdx].EndByte <= lineStart) {
				currentRangeStartIdx++;
			}

			// 2. この行の「開始インデックス」を記録
			int startIdx = currentRangeStartIdx;
			int endIdx = currentRangeStartIdx;

			// 3. この行に関係のあるハイライトがどこまで続くか（終了インデックス）を調べる
			for (int k = currentRangeStartIdx; k < rangeCount; k++) {
				var range = sortedRanges[k];

				// この行の終わり以降にあるハイライトに到達したらストップ
				if (range.StartByte >= lineEnd) {
					break;
				}

				// 行と交差している場合、終了位置を更新していく
				if (range.EndByte > lineStart && range.StartByte < lineEnd) {
					endIdx = k + 1; // 該当するインデックスの「次の位置」を終了境界にする（for文の条件で未満にするため）
				}
			}

			// もし1件も交差しなかった場合は startIdx と endIdx が同じ値になり、JS側で件数0と判定できる
			lineRangeBoundaries.Add(startIdx);
			lineRangeBoundaries.Add(endIdx);
		}

		// JavaScript側への出力（以前の lineHighlight... は削除してこれ1本にします）

		//byte[] utf16Bytes = Encoding.Unicode.GetBytes(body);
		//// 3. JavaScript側への一斉出力
		//writer.WriteLine($"const fileTextBase64 = \"{Convert.ToBase64String(utf16Bytes)}\";");
		//writer.WriteLine("const binary = atob(fileTextBase64);");
		//writer.WriteLine("const bytes = Uint8Array.from(binary, c => c.charCodeAt(0));");
		//writer.WriteLine("const fileText = new TextDecoder(\"utf-16le\").decode(bytes);");

		var parts = body.Split(new[] { "</script>" }, StringSplitOptions.None);
		for (int i = 0; i < parts.Length; i++) {
			writer.WriteLine(
				$"<script class=\"rawdata\" type=\"text/plain\">{ SafeHtmlEscape( parts[i])}</script>");
		}
		
		writer.WriteLine();
		writer.WriteLine("<script>");
		writer.WriteLine(EnumExtensions.GetJsEnumString<LanguageId>());


		//writer.WriteLine($"const lineStartOffsets = [{string.Join(",", lineStartOffsets)}];");

		var values = lineStartOffsets.ToArray();
		writer.Write("const lineStartOffsets = [");
		for (int i = 0; i < values.Length; i++) {
			if (i > 0)
				writer.Write(",");
			if (i > 0 && i % 50 == 0)
				writer.WriteLine();
			writer.Write(values[i]);
		}
		writer.WriteLine("];");


		writer.WriteLine($"const highlightRanges = [{rangeSb.ToString()}];");
		writer.WriteLine($"const captureNames = {mapSb.ToString()};");

		// writer.WriteLine($"const utf8Index = {utfIndexSb.ToString()};");

		//writer.WriteLine($"const lineRangeBoundaries = [{string.Join(",", lineRangeBoundaries)}];");
		values = lineRangeBoundaries.ToArray();
		writer.Write("const lineRangeBoundaries = [");
		for (int i = 0; i < values.Length; i++) {
			if (i > 0)
				writer.Write(",");
			if (i > 0 && i % 50 == 0)
				writer.WriteLine();
			writer.Write(values[i]);
		}
		writer.WriteLine("];");


		//sw.Stop();
		//Console.WriteLine($"書き出し:{sw.ElapsedMilliseconds / 1000.0:F2}");




		if (result != null &&  utf8Body.Length <= 100 * 1024 * 1024) {
			var langEleList = BuildTree(langDef.PrimaryName, rootNode, utf8Body);
			CalcLineNumber(langEleList, utf8Body);

			for (int i = 0; i < langEleList.Count; i++) {
				result.Langs.Add(new CodeAnalysisLangResult { ElementRoot = langEleList[i] });

				LanguageDefinition def = LanguageRegistry.GetByName(result.Langs[i].ElementRoot.Language );
				if (def?.Parse != null) {
					var symbols = def.Parse(def, result.Langs[i]);
					result.Langs[i].Symbols = symbols;
					// C#だと片方で全てstartbyte/endbyteが設定されるが、htmlはSymbolsとRootは同じノードになっていないので両方必要 
					ChangeUtf8to16Symbol(utf8Body, map, result.Langs[i].Symbols, result.FileId);
					ChangeUtf8to16Symbol(utf8Body, map, result.Langs[i].Root, result.FileId);
				}
			}

			// result.ElementRoots = BuildTree(langDef.PrimaryName, rootNode, utf8Body);

			//if (result.ElementRoots != null) {
			//	if (langDef.Parse != null) {
			//		var symbols = langDef.Parse(langDef, result);
			//		result.Symbols = symbols;

			//		// C#だと片方で全てstartbyte/endbyteが設定されるが、htmlはSymbolsとRootは同じノードになっていないので両方必要 
			//		ChangeUtf8to16Symbol(map, result.Symbols);
			//		ChangeUtf8to16Symbol(map, result.Root);
			//	}
			//}
		}


		return true;
	}

	public static void ChangeUtf8to16(int[] map, int[] lineStartOffsets, List<HighlightRange> sortedRanges)
	{
		
		for (int i = 0; i < lineStartOffsets.Length; i++) {
			lineStartOffsets[i] = map[lineStartOffsets[i]];
		}
		for (int i = 0; i < sortedRanges.Count; i++) {
			var r = sortedRanges[i];

			r.StartByte = map[r.StartByte];
			r.EndByte = map[r.EndByte];

			sortedRanges[i] = r;
		}
	}
	public static void ChangeUtf8to16Symbol(byte[] utf8Body, int[] map, List<SymbolMatch> list, string fileId)
	{
		if (list == null || list.Count <= 0) {
			return;
		}
		for (int i = 0; i < list.Count; i++) {
			list[i].StartByte = map[list[i].Node.StartByte];
			list[i].EndByte = map[list[i].Node.EndByte];
			list[i].FileId = fileId;
			for (int k = 0; k < list[i].CallInfos.Count; k++) {
				list[i].CallInfos[k].UsageContext = GetContext(utf8Body, list[i].CallInfos[k].Node.StartByte, list[i].CallInfos[k].Node.EndByte);
				list[i].CallInfos[k].StartByte = map[list[i].CallInfos[k].Node.StartByte];
				list[i].CallInfos[k].EndByte = map[list[i].CallInfos[k].Node.EndByte];
				list[i].CallInfos[k].FileId = fileId;
			}
		}
	}
	public static void ChangeUtf8to16Symbol(byte[] utf8Body, int[] map, SymbolMatch sym, string fileId)
	{
		for (int k = 0; k < sym.CallInfos.Count; k++) {
			sym.CallInfos[k].UsageContext = GetContext(utf8Body, sym.CallInfos[k].Node.StartByte, sym.CallInfos[k].Node.EndByte);
			sym.CallInfos[k].StartByte = map[sym.CallInfos[k].Node.StartByte];
			sym.CallInfos[k].EndByte = map[sym.CallInfos[k].Node.EndByte];
			sym.CallInfos[k].FileId = fileId;
		}
		if (sym.Node != null) {
			sym.StartByte = map[sym.Node.StartByte];
			sym.EndByte = map[sym.Node.EndByte];
			sym.FileId = fileId;
		}
		for (int i = 0; i < sym.Childs.Count; i++) {
			ChangeUtf8to16Symbol(utf8Body, map, sym.Childs[i], fileId);
		}

	}



	public static int[] BuildUtf8ToUtf16Map(string text)
	{
		int utf8Length = Encoding.UTF8.GetByteCount(text);

		var map = new int[utf8Length + 1];

		int utf8Pos = 0;
		int utf16Pos = 0;

		for (int i = 0; i < text.Length; i++) {
			char c = text[i];

			int utf8Len;

			// ASCII
			if (c <= 0x7F) {
				utf8Len = 1;
			}
			// 2-byte UTF8
			else if (c <= 0x7FF) {
				utf8Len = 2;
			}
			// surrogate pair
			else if (char.IsHighSurrogate(c) &&
					 i + 1 < text.Length &&
					 char.IsLowSurrogate(text[i + 1])) {
				utf8Len = 4;

				map[utf8Pos++] = utf16Pos;
				map[utf8Pos++] = utf16Pos;
				map[utf8Pos++] = utf16Pos;
				map[utf8Pos++] = utf16Pos;

				utf16Pos += 2;
				i++;

				continue;
			}
			// 3-byte UTF8
			else {
				utf8Len = 3;
			}

			for (int j = 0; j < utf8Len; j++)
				map[utf8Pos + j] = utf16Pos;

			utf8Pos += utf8Len;
			utf16Pos++;
		}
		map[utf8Pos] = utf16Pos;
		return map;
	}

	public static bool ParseSymbols(IntPtr language, TSNode rootNode, byte[] utf8Body, string scmPath, List<TreeSitterSymbolInfo> symbol_list)
	{
		if (!File.Exists(scmPath))
			return false;

		var lineStarts = BuildLineIndex(utf8Body);

		byte[] scmSource = File.ReadAllBytes(scmPath);
		IntPtr query = ts_query_new(language, scmSource, (uint)scmSource.Length, out uint errorOffset, out TSQueryError errorType);

		if (query == IntPtr.Zero) {
			Console.WriteLine($"Query Error: {errorType} at offset {errorOffset}");
			string errorPart = System.Text.Encoding.UTF8.GetString(scmSource)
						.Substring((int)errorOffset);
			Console.WriteLine($"Query Error: {errorType} around here: {errorPart}");
			return false;
		}
		IntPtr cursor = ts_query_cursor_new();

		try {
			ts_query_cursor_exec(cursor, query, rootNode);

			TSQueryMatch match;
			while (ts_query_cursor_next_match(cursor, out match)) {
				TSNode symbolNode = default(TSNode); // シンボル本体（行番号用）
				string kind = "";                // definition.function など
				string name = "";                // 関数名やクラス名そのもの

				for (int i = 0; i < match.capture_count; i++) {
					var capture = (TSQueryCapture)Marshal.PtrToStructure(
						new IntPtr(match.captures.ToInt64() + (i * Marshal.SizeOf(typeof(TSQueryCapture)))),
						typeof(TSQueryCapture)
					);

					uint nameLength;
					IntPtr namePtr = ts_query_capture_name_for_id(query, capture.index, out nameLength);
					string captureName = Marshal.PtrToStringAnsi(namePtr);

					string temp_name = Substring(utf8Body, (int)ts_node_start_byte(capture.node), (int)ts_node_end_byte(capture.node));

					if (captureName != "local.definition.parameter") {
						bool need_braek = true;
					}

					// @name キャプチャがある場合は、それをシンボル名として採用
					if (captureName == "name") {
						//name = Substring(utf8Body, (int)ts_node_start_byte(capture.node), (int)ts_node_end_byte(capture.node));
						name = temp_name;
					}
					// @definition.xxx や @reference.xxx を型（種類）として採用
					//else if (captureName.Contains("definition") || captureName.Contains("reference")) {
					else if (captureName.Contains("definition")) {
						kind = captureName;
						// @name が定義されていない場合、このノードの位置を基準にする
						if (symbolNode.id == IntPtr.Zero)
							symbolNode = capture.node;
					}
				}

				// 最低限「種類」が取れていればリストに追加
				if (!string.IsNullOrEmpty(kind)) {
					int startPos = (int)ts_node_start_byte(symbolNode);
					symbol_list.Add(new TreeSitterSymbolInfo {
						Type = kind, // definition.method など
						Name = string.IsNullOrEmpty(name) ? "anonymous" : name,
						LineNumber = GetLine(startPos, lineStarts)
					});
				}
			}
		} catch (Exception e) {
			Console.WriteLine("例外: " + e.ToString());
		} finally {
			if (cursor != IntPtr.Zero) {
				ts_query_cursor_delete(cursor);
				cursor = IntPtr.Zero;
			}
			if (query != IntPtr.Zero) {
				ts_query_delete(query);
				query = IntPtr.Zero;
			}
		}
		return true;
	}

	static int[] BuildLineIndex(byte[] text)
	{
		var list = new List<int>();
		list.Add(0);

		for (int i = 0; i < text.Length; i++) {
			if (text[i] == (byte)'\n') {
				list.Add(i + 1);
			}
		}

		return list.ToArray();
	}


	static int GetLine(int bytePos, int[] lineStarts)
	{
		int line = Array.BinarySearch(lineStarts, bytePos);
		if (line < 0)
			line = ~line - 1;
		return line + 1;
	}

	public static List<HighlightRange> GetRangeList(string lang, string body, int positionOffset = 0)
	{
		var langDef = LanguageRegistry.GetByExtension(lang);
		if (langDef == null) {
			langDef = LanguageRegistry.GetByName(lang);
		}
		IntPtr parser = IntPtr.Zero;
		IntPtr tree = IntPtr.Zero;
		try {
			byte[] utf8Body = Encoding.UTF8.GetBytes(body);
			parser = ts_parser_new();
			ts_parser_set_language(parser, langDef.GetLanguage());
			tree = ts_parser_parse_string(parser, IntPtr.Zero, utf8Body, (uint)utf8Body.Length);
			var rootNode = ts_tree_root_node(tree);
			return GetRangeList(rootNode, langDef, body, positionOffset);
		} catch (Exception e) {
			Console.WriteLine(e.ToString());
		} finally {
			ts_tree_delete(tree);
			ts_parser_delete(parser);
		}
		return null;
	}
	public static List<HighlightRange> GetRangeList(TSNode rootNode,  LanguageDefinition langDef, string body, int positionOffset = 0)
	{
		return GetRangeList(rootNode, langDef, Encoding.UTF8.GetBytes(body), positionOffset);
	}
	public static List<HighlightRange> GetRangeList(TSNode rootNode, LanguageDefinition langDef, byte[] utf8Body, int positionOffset = 0)
	{
		if (langDef == null) {
			return null;
		}
		List<HighlightRange> list = new List<HighlightRange>();
		try {
			var temp_list = ProcessInjQuery(langDef.GetLanguage(), rootNode, utf8Body,
				GetInjectionsScmPath(langDef), positionOffset);
			if (temp_list != null) {
				list.AddRange(temp_list);
			}
			temp_list = GetRangeList(langDef.GetLanguage(), rootNode,
				GetHighlightsScmPath(langDef), positionOffset);
			if (temp_list != null) {
				list.AddRange(temp_list);
			}
		} catch (Exception e) {
			Console.WriteLine(e.ToString());
		}
		return list;
	}


	public static List<HighlightRange> GetRangeList(IntPtr language, TSNode rootNode, string scmPath, int positionOffset = 0)
	{
		var ranges = new List<HighlightRange>();
		byte[] scmSource = File.ReadAllBytes(scmPath);
		IntPtr query = ts_query_new(language, scmSource, (uint)scmSource.Length, out uint errorOffset, out TSQueryError errorType);

		if (query == IntPtr.Zero)
			return null;

		IntPtr cursor = ts_query_cursor_new();
		try {

			ts_query_cursor_exec(cursor, query, rootNode);

			HighlightRange lastRange = default(HighlightRange);
			TSQueryMatch match;
			int structSize = Marshal.SizeOf(typeof(TSQueryCapture));
			// ループ外で1回だけサイズを取得（JIT最適化されやすいように）

			while (ts_query_cursor_next_match(cursor, out match)) {
				int captureCount = match.capture_count;
				if (captureCount == 0)
					continue;

				unsafe {
					byte* pBase = (byte*)match.captures.ToPointer();

					for (int i = 0; i < captureCount; i++) {
						TSQueryCapture capture = *(TSQueryCapture*)(pBase + (i * structSize));

						// 【超爆速化①】DLLを呼ばず、構造体から直接開始位置を取る！
						int startByte = (int)capture.node.context0 + positionOffset;

						// 【超爆速化②】終了位置はセキュリティ解除した最速DLL呼び出し（回数も従来の半分）
						int endByte = (int)ts_node_end_byte(capture.node) + positionOffset;

						// 重複チェック
						if (lastRange.CaptureName != null &&
							startByte == lastRange.StartByte &&
							endByte == lastRange.EndByte) {
							continue;
						}

						// 文字列変換（必要なら戻してください、不要なら固定文字のままでOK）
						uint nameLength;
						IntPtr namePtr = ts_query_capture_name_for_id(query, capture.index, out nameLength);
						string captureName = Marshal.PtrToStringAnsi(namePtr);

						var newRange = new HighlightRange {
							StartByte = startByte,
							EndByte = endByte,
							CaptureName = captureName
						};

						ranges.Add(newRange);
						lastRange = newRange;
					}
				}
			}
		} catch (Exception e) {
			Console.WriteLine("例外: " + e.ToString());
		} finally {
			if (cursor != IntPtr.Zero) {
				ts_query_cursor_delete(cursor);
				cursor = IntPtr.Zero;
			}
			if (query != IntPtr.Zero) {
				ts_query_delete(query);
				query = IntPtr.Zero;
			}
		}
		return ranges;
	}
	private static uint GetNodeLength(TSNode node)
	{
		// 多くのバージョンでは context3 にそのノード自体のバイト長（あるいはパディング込みの長さ）が入っています
		// ts_node_end_byte(node) - ts_node_start_byte(node) の値と一致するか確認してみてください
		return node.context1;
	}


	public static List<HighlightRange> GetRangeList_org(IntPtr language, TSNode rootNode, string scmPath, int positionOffset = 0)
	{
		var ranges = new List<HighlightRange>();
		byte[] scmSource = File.ReadAllBytes(scmPath);
		IntPtr query = ts_query_new(language, scmSource, (uint)scmSource.Length, out uint errorOffset, out TSQueryError errorType);

		if (query == IntPtr.Zero)
			return null;

		IntPtr cursor = ts_query_cursor_new();
		try {

			ts_query_cursor_exec(cursor, query, rootNode);

			HighlightRange lastRange = default(HighlightRange);
			TSQueryMatch match;
			while (ts_query_cursor_next_match(cursor, out match)) {
				// キャプチャ（マッチした箇所）をスキャン
				for (int i = 0; i < match.capture_count; i++) {
					// ポインタ演算で TSQueryCapture を取得
					var capture = (TSQueryCapture)Marshal.PtrToStructure(
						new IntPtr(match.captures.ToInt64() + (i * Marshal.SizeOf(typeof(TSQueryCapture)))),
						typeof(TSQueryCapture)
					);

					uint nameLength;
					IntPtr namePtr = ts_query_capture_name_for_id(query, capture.index, out nameLength);
					string captureName = Marshal.PtrToStringAnsi(namePtr);

					int startByte = (int)ts_node_start_byte(capture.node);
					int endByte = (int)ts_node_end_byte(capture.node);


					// 同じ文字列に対して複数のヒットがあるが、一番最初のものを採用する。 
					bool isSameExistingRange = lastRange.CaptureName != null &&
								 (startByte + positionOffset) == lastRange.StartByte &&
								 (endByte + positionOffset) == lastRange.EndByte;

					if (!isSameExistingRange) {
						var newRange = new HighlightRange {
							StartByte = startByte + positionOffset,
							EndByte = endByte + positionOffset,
							CaptureName = captureName
						};
						ranges.Add(newRange);
						lastRange = newRange; // 直前データを更新
					}
				}
			}

		} catch (Exception e) {
			Console.WriteLine("例外: " + e.ToString());
		} finally {
			if (cursor != IntPtr.Zero) {
				ts_query_cursor_delete(cursor);
				cursor = IntPtr.Zero;
			}
			if (query != IntPtr.Zero) {
				ts_query_delete(query);
				query = IntPtr.Zero;
			}
		}
		return ranges;
	}


	// HighlightRangeのAの中にHighlightRangeのBが存在してる場合にはAを分割して対象オフセットが被らないようにする。 
	// cssだと"100px"というrangeが来た後、"px"が来てこのpxの位置は100pxのpxと同じ位置を指す。  
	public static List<HighlightRange> FlattenAndSplitRanges(List<HighlightRange> rawRanges)
	{
		if (rawRanges.Count == 0)
			return rawRanges;

		// 1. 「開始位置」の昇順でソート。
		//    もし開始位置が同じなら、「終了位置が後ろ（＝外側の親）」を先にする。
		var sorted = rawRanges
			.OrderBy(r => r.StartByte)
			.ThenByDescending(r => r.EndByte)
			.ToList();

		var result = new List<HighlightRange>();

		// 2. スキャン用のスタック（親要素を記憶しておく箱）
		var stack = new Stack<HighlightRange>();
		int currentByte = 0;

		foreach (var next in sorted) {
			// 現在位置が、処理しようとしているノードの手前にある場合
			while (stack.Count > 0 && currentByte < next.StartByte) {
				var parent = stack.Peek();

				// 親の終わりが、次のノードの開始より手前、または同じなら、親の未出力部分を吐き出す
				if (parent.EndByte <= next.StartByte) {
					if (currentByte < parent.EndByte) {
						result.Add(new HighlightRange {
							StartByte = currentByte,
							EndByte = parent.EndByte,
							CaptureName = parent.CaptureName
						});
						currentByte = parent.EndByte;
					}
					stack.Pop(); // この親は完全に処理終了
				} else {
					// 次のノードの直前まで、親の色で埋める
					result.Add(new HighlightRange {
						StartByte = currentByte,
						EndByte = next.StartByte,
						CaptureName = parent.CaptureName
					});
					currentByte = next.StartByte;
					break;
				}
			}

			// スタックの整理（すでに終わっている親を捨てる）
			while (stack.Count > 0 && stack.Peek().EndByte <= next.StartByte) {
				stack.Pop();
			}

			// 隙間があれば、色なし（または地の文）として進める（通常はTree-sitterがカバーするのでほぼ通らない）
			if (currentByte < next.StartByte) {
				currentByte = next.StartByte;
			}

			// 新しいノード（子）を処理
			if (currentByte < next.EndByte) {
				// もし既存の処理位置より、次のノードの開始が後ろなら、
				// その手前までを現在の色（親の色など）で確定させてから子を入れる
				stack.Push(next);
			}
		}

		// 最後にスタックに残った親たちの残党を後ろから順番に吐き出す
		while (stack.Count > 0) {
			var parent = stack.Pop();
			if (currentByte < parent.EndByte) {
				result.Add(new HighlightRange {
					StartByte = currentByte,
					EndByte = parent.EndByte,
					CaptureName = parent.CaptureName
				});
				currentByte = parent.EndByte;
			}
		}

		return result;
	}


	static List<HighlightRange> ProcessInjQuery(IntPtr language, TSNode rootNode, byte[] utf8Body, string scmPath, int positionOffset = 0)
	{
		List<HighlightRange> list = new List<HighlightRange>();

		if (!File.Exists(scmPath))
			return list;

		var lineStarts = BuildLineIndex(utf8Body);

		byte[] scmSource = File.ReadAllBytes(scmPath);
		IntPtr query = ts_query_new(language, scmSource, (uint)scmSource.Length, out uint errorOffset, out TSQueryError errorType);

		if (query == IntPtr.Zero) {
			return list;
		}
		IntPtr cursor = ts_query_cursor_new();

		var injectionTargets = 	new List<(string Name, List<TSRange> Ranges)>();

		try {
			ts_query_cursor_exec(cursor, query, rootNode);

			TSQueryMatch match;
			while (ts_query_cursor_next_match(cursor, out match)) {
				for (int i = 0; i < match.capture_count; i++) {
					var capture = (TSQueryCapture)Marshal.PtrToStructure(
						new IntPtr(match.captures.ToInt64() + (i * Marshal.SizeOf(typeof(TSQueryCapture)))),
						typeof(TSQueryCapture)
					);

					uint nameLength;
					IntPtr namePtr = ts_query_capture_name_for_id(query, capture.index, out nameLength);
					string captureName = Marshal.PtrToStringAnsi(namePtr);

					int startByte = (int)ts_node_start_byte(capture.node);
					int endByte = (int)ts_node_end_byte(capture.node);

					string text = Substring(utf8Body, startByte, endByte);
					string type = Marshal.PtrToStringAnsi(ts_node_type(capture.node));
					var point = ts_node_start_point(capture.node);

					if (captureName == "injection.content") {
						// TSQueryPredicateStep  
						IntPtr ptrPredicate =  ts_query_predicates_for_pattern(query, match.pattern_index, out var length);
						int size = Marshal.SizeOf<TSQueryPredicateStep>();

						string lang_name = "";
						bool next_lang_name = false;
						for (int k = 0; k < length; k++) {
							IntPtr p = IntPtr.Add(ptrPredicate, k * size);
							TSQueryPredicateStep step = Marshal.PtrToStructure<TSQueryPredicateStep>(p);
							// Console.WriteLine($"{step.type} {step.value_id}");
							if (step.type == TSQueryPredicateStepType.TSQueryPredicateStepTypeString) {
								IntPtr strPtr = ts_query_string_value_for_id( query, step.value_id, out var len);
								string s = 	Marshal.PtrToStringAnsi(strPtr, (int)len);
								// Console.WriteLine(s);
								if (next_lang_name == false) {
									if (s == "injection.language") {
										next_lang_name = true;
									}
								} else {
									lang_name = s;
									break;
								}
							}
						}

						if (!string.IsNullOrEmpty(lang_name)) {
							var ranges = injectionTargets.FirstOrDefault(x => x.Name == lang_name).Ranges;
							if (ranges == null) {
								ranges = new List<TSRange>();
								injectionTargets.Add((lang_name, ranges));
							}
							ranges.Add(new TSRange { start_byte = (uint)startByte, end_byte = (uint)endByte });
						}

					} 
				}
			}

			// 最後に合わせてパースしないと、例えば.phpで</ul>だけのtextをhtmlパーサーに渡すとエラーになる。 
			// 書き方によっては最後に合わせてパースしてもエラーになる場合はあるが、少しマシになる 
			IntPtr parser = IntPtr.Zero;
			IntPtr tree = IntPtr.Zero;
			try {

				foreach (var target in injectionTargets) {
					// リストに中身があるときだけ処理
					if (target.Ranges.Count > 0) {
						if (parser == IntPtr.Zero) {
							parser = ts_parser_new();
						}

						var injLangDef = LanguageRegistry.GetByName(target.Name);
						if (injLangDef == null) {
							continue;
						}
						var injLang = injLangDef.GetLanguage();

						ts_parser_set_language(parser, injLang);
						ts_parser_set_included_ranges(parser, target.Ranges.ToArray(), (uint)target.Ranges.Count);

						tree = ts_parser_parse_string(parser, IntPtr.Zero, utf8Body, (uint)utf8Body.Length);
						var injRoot = ts_tree_root_node(tree);

						var injList = GetRangeList(injRoot, injLangDef, utf8Body);
						if (injList != null) {
							list.AddRange(injList);
						}

						// メモリ解放もループ内でバッチリ
						ts_tree_delete(tree);
						tree = IntPtr.Zero;
					}
				}
			} catch (Exception e) {
				Console.WriteLine(e.ToString());
			} finally {
				ts_tree_delete(tree);
				ts_parser_delete(parser);
			}



		} catch (Exception e) {
			Console.WriteLine("例外: " + e.ToString());
		} finally {
			if (cursor != IntPtr.Zero) {
				ts_query_cursor_delete(cursor);
				cursor = IntPtr.Zero;
			}
			if (query != IntPtr.Zero) {
				ts_query_delete(query);
				query = IntPtr.Zero;
			}
		}

		return list;
	}




	static string SafeHtmlEscape(string text)
	{
		return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
		   .Replace("\"", "&quot;").Replace("'", "&#39;");
	}
	private static string Substring(byte[] utf8Body, int start, int end)
	{
		if (start >= end)
			return string.Empty;
		return Encoding.UTF8.GetString(utf8Body, start, end - start);
	}


	public static string GetContext(byte[] utf8Body, int start, int end)
	{
		if (utf8Body == null || utf8Body.Length == 0)
			return string.Empty;

		if (start < 0)
			start = 0;

		if (end < start)
			end = start;

		if (start > utf8Body.Length)
			start = utf8Body.Length;

		if (end > utf8Body.Length)
			end = utf8Body.Length;

		// start より前にある \n を探す
		int contextStart = start;
		while (contextStart > 0 && utf8Body[contextStart - 1] != (byte)'\n')
			contextStart--;

		// end より後にある \n を探す
		int contextEnd = end;
		while (contextEnd < utf8Body.Length && utf8Body[contextEnd] != (byte)'\n')
			contextEnd++;

		// 1行が長すぎる場合は最大長に制限
		const int MaxContextLength = 50;

		int length = contextEnd - contextStart;

		if (length <= 0)
			return string.Empty;

		if (length > MaxContextLength) {
			// start/end付近を優先して切り出す
			int center = (start + end) / 2;
			int half = MaxContextLength / 2;

			contextStart = Math.Max(0, center - half);
			contextEnd = Math.Min(utf8Body.Length, contextStart + MaxContextLength);

			length = contextEnd - contextStart;
		}

		return Encoding.UTF8.GetString(utf8Body, contextStart, length).Trim();
	}

	public static string GetContext_org(byte[] utf8Body, int start, int end)
	{
		if (utf8Body == null || utf8Body.Length == 0)
			return string.Empty;

		if (start < 0)
			start = 0;

		if (end < start)
			end = start;

		if (start > utf8Body.Length)
			start = utf8Body.Length;

		if (end > utf8Body.Length)
			end = utf8Body.Length;

		// start より前にある \n を探す
		int contextStart = start;
		while (contextStart > 0 && utf8Body[contextStart - 1] != (byte)'\n') {
			contextStart--;
		}

		// end より後にある \n を探す
		int contextEnd = end;
		while (contextEnd < utf8Body.Length && utf8Body[contextEnd] != (byte)'\n') {
			contextEnd++;
		}

		int length = contextEnd - contextStart;

		if (length <= 0)
			return string.Empty;

		return Encoding.UTF8.GetString(utf8Body, contextStart, length).Trim();
	}


	public struct HighlightRange
	{
		public int StartByte;
		public int EndByte;
		public string CaptureName;
	}




	public class SymbolInfo
	{
		public string Name;
		public string Type; // Class, Method, etc.
		public int StartByte;
	}

	public static void ExtractSymbols(TSNode node, byte[] utf8Body, List<SymbolInfo> symbols)
	{
		string type = Marshal.PtrToStringAnsi(ts_node_type(node));

		// 親ノードが「定義」を表すタイプかチェック
		if (type == "class_declaration" || type == "method_declaration") {
			// 子ノードの中から "identifier" を探して名前を取得
			// (フィールド名 "name" を指定して取るのが一番確実)
			byte[] utf_byte = Encoding.UTF8.GetBytes("name");
			TSNode nameNode = ts_node_child_by_field_name(node, utf_byte, (uint)utf_byte.Length);

			if (!ts_node_is_null(nameNode)) {
				int start = (int)ts_node_start_byte(nameNode);
				int end = (int)ts_node_end_byte(nameNode);

				symbols.Add(new SymbolInfo {
					Name = Encoding.UTF8.GetString(utf8Body, start, end - start),
					Type = type,
					StartByte = start
				});
			}
		}

		// 再帰的に全部見る
		uint childCount = ts_node_child_count(node);
		for (uint i = 0; i < childCount; i++) {
			ExtractSymbols(ts_node_child(node, i), utf8Body, symbols);
		}
	}
	public struct BlockRange
	{
		public int StartByte;
		public int EndByte;
		public string Content;
	}
	public static void FindAllBlocks(TSNode node, byte[] utf8Body, List<BlockRange> list)
	{
		if (ts_node_is_null(node))
			return;

		// もし現在のノードが「ブロック」ならリストに保存
		string typeName = Marshal.PtrToStringAnsi(ts_node_type(node));
		// declaration_list 
		if (typeName == "statement_block" || typeName == "block") {
			int start = (int)ts_node_start_byte(node);
			int end = (int)ts_node_end_byte(node);

			list.Add(new BlockRange {
				StartByte = start,
				EndByte = end,
				Content = Encoding.UTF8.GetString(utf8Body, start, end - start)
			});
		}

		// 子ノードをすべて再帰的に探索
		uint childCount = ts_node_child_count(node);
		for (uint i = 0; i < childCount; i++) {
			TSNode child = ts_node_child(node, i);
			FindAllBlocks(child, utf8Body, list); // 深く掘り下げる
		}
	}

	public struct TreeSitterElementFlat
	{
		public string TypeName;
		public int StartByte;
		public int EndByte;
		public string Content;

	}
	public static void FindAllFlatElement(TSNode node, byte[] utf8Body, List<TreeSitterElementFlat> list)
	{
		if (ts_node_is_null(node))
			return;

		// もし現在のノードが「ブロック」ならリストに保存
		string typeName = Marshal.PtrToStringAnsi(ts_node_type(node));
			int start = (int)ts_node_start_byte(node);
			int end = (int)ts_node_end_byte(node);

			list.Add(new TreeSitterElementFlat {
				TypeName = typeName,
				StartByte = start,
				EndByte = end,
				Content = Encoding.UTF8.GetString(utf8Body, start, end - start)
			});
		

		// 子ノードをすべて再帰的に探索
		uint childCount = ts_node_child_count(node);
		for (uint i = 0; i < childCount; i++) {
			TSNode child = ts_node_child(node, i);
			FindAllFlatElement(child, utf8Body, list); // 深く掘り下げる
		}
	}
	public class TreeSitterElement
	{
		public string TypeName;
		public int StartByte;
		public int EndByte;
		public int LineNumber { get; set; }
		public string Content;

		public string Language;

		public string FieldName;
		public bool IsNamed;

		// 親ノードへの参照（nullの場合はルートノード）
		public TreeSitterElement Papa;

		// 子ノードたちのリスト（インスタンス化しておく）
		public List<TreeSitterElement> Childs = new List<TreeSitterElement>();
	}



	public class TreeSitterElementInjections
	{
		public Dictionary<string, List<TreeSitterElement>> Dic = new Dictionary<string, List<TreeSitterElement>>();

		public void Add(string lang, TreeSitterElement ele)
		{
			if (!Dic.ContainsKey(lang)) {
				Dic[lang] = new List<TreeSitterElement>();
			}
			Dic[lang].Add(ele);
		}

	}


	public static void CalcLineNumber(List<TreeSitterElement> allLangRoot, byte[] utf8Body)
	{
		var lineStarts = BuildLineIndex(utf8Body);
		foreach (var item in allLangRoot) {
			DoCalcWork(item, lineStarts);
		}
	}

	static void DoCalcWork(TreeSitterElement node, int[] lineStarts)
	{
		node.LineNumber = GetLine(node.StartByte, lineStarts);
		if (node.Childs == null) {
			return;
		}
		foreach (var c in node.Childs) {
			DoCalcWork(c, lineStarts);
		}
	}


	public static List<TreeSitterElement> BuildTree(string lang, TSNode rootNode, byte[] utf8Body, int offset = 0)
	{
		List<TreeSitterElement> allLang = new List<TreeSitterElement>();


		var inj = new TreeSitterElementInjections();
		var mainRoot = FindAllElement(lang, rootNode, utf8Body, null, null, inj, offset);
		allLang.Add(mainRoot);

		IntPtr parser = IntPtr.Zero;
		IntPtr tree = IntPtr.Zero;
		try {
			// ループ用に（言語名, 対象のリスト）のペアを配列にする

			foreach (var item in inj.Dic) {
				if (parser == IntPtr.Zero) {
					parser = ts_parser_new();
				}

				// elements が List<TreeSitterElement> だとします
				TSRange[] ranges = item.Value.Select(e => new TSRange {
					start_byte = (uint)e.StartByte,
					end_byte = (uint)e.EndByte,
					// もし TSRange 構造体に start_point や end_point がある場合はここで行・列も設定します
				}).ToArray();

				var injLangDef = LanguageRegistry.GetByName(item.Key);
				var injLang = injLangDef.GetLanguage();

				ts_parser_set_language(parser, injLang);
				ts_parser_set_included_ranges(parser, ranges, (uint)ranges.Length);

				tree = ts_parser_parse_string(parser, IntPtr.Zero, utf8Body, (uint)utf8Body.Length);
				var injRoot = ts_tree_root_node(tree);

				var injElementRoot = BuildTree(item.Key, injRoot, utf8Body);
				allLang.AddRange(injElementRoot);
				
				// メモリ解放もループ内でバッチリ
				ts_tree_delete(tree);
				tree = IntPtr.Zero;
			}

		} catch (Exception e) {
			Console.WriteLine(e.ToString());
		} finally {
			ts_tree_delete(tree);
			ts_parser_delete(parser);
		}

		return allLang;

	}

	// 引数に「papa（親）」を渡せるようにして、再帰の中で親子関係を結びつけます
	private static TreeSitterElement FindAllElement(string lang, TSNode node, byte[] utf8Body, string fieldname, 
			TreeSitterElement papa, TreeSitterElementInjections inj, int offset = 0)
	{
		if (ts_node_is_null(node))
			return null;

		string typeName = Marshal.PtrToStringAnsi(ts_node_type(node));
		int start = (int)ts_node_start_byte(node);
		int end = (int)ts_node_end_byte(node);

		// 1. 自分自身のインスタンスを作成
		var current = new TreeSitterElement {
			Language = lang,
			TypeName = typeName,
			StartByte = start + offset,
			EndByte = end + offset,
			IsNamed = ts_node_is_named(node),
			FieldName = fieldname,
			Content = Encoding.UTF8.GetString(utf8Body, start, end - start),
			Papa = papa // 引数でもらった親をセット
		};
		if (lang == "html" && typeName == "raw_text") {
			if (papa?.TypeName == "style_element") {
				inj.Add("css", current);
			} else if (papa?.TypeName == "script_element") {
				inj.Add("javascript", current);
			}
		} else if (lang == "php" && typeName == "text") {
			inj.Add("html", current);
		} else if (lang == "rust" && typeName == "token_tree") {
			// rustは範囲をリスト化して最後に ts_parser_set_included_rangesをやる方法は無理。
			// とういうかマクロ内にマクロがあったりする。 token_treeの下にノードがある。
			//inj.Add("rust", current);
			//return current;
		}



		// 2. 子ノードをすべて再帰的に探索
		uint childCount = ts_node_child_count(node);
		for (uint i = 0; i < childCount; i++) {
			TSNode childNode = ts_node_child(node, i);

			IntPtr pField = ts_node_field_name_for_child(node, i);
			string childFieldName = pField == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(pField);

			// 自分（current）を「親」として渡して、子ノードのツリーを作ってもらう
			var childElement = FindAllElement(lang, childNode, utf8Body, childFieldName, current, inj,  offset);

			if (childElement != null) {
				// 3. 戻ってきた子ノードを、自分のChildsリストに追加
				current.Childs.Add(childElement);
			}
		}

		// 最後に自分自身を返す
		return current;
	}
	public static string ToHtmlTree(TreeSitterElement element)
	{
		if (element == null)
			return "";

		var sb = new StringBuilder();
		string safeContent = System.Web.HttpUtility.HtmlEncode(element.Content);
		string len = $"　<span class='range'>[{element.LineNumber}]</span>";		// 行番号の方が意味ある 
		string field = "";
		if (!string.IsNullOrEmpty(element.FieldName)) {
			field = $"<span class='field'>({element.FieldName})</span>";
		}
		string color = "";
		if (element.IsNamed) {
			color = " is-named";
		}

		string safeType = System.Web.HttpUtility.HtmlEncode($"{element.TypeName} {element.FieldName}");

		if (element.Childs.Count > 0) {
			sb.AppendLine("<details style='margin-left: 15px;'>");

			sb.AppendLine($"  <summary><span class='tree-item {color}' data-content='{safeContent}' data-type='{safeType}' data-line='{element.LineNumber}'><b>{HttpUtility.HtmlEncode(element.TypeName)}{field}{len}</b></span></summary>");
			foreach (var child in element.Childs) {
				sb.AppendLine(ToHtmlTree(child));
			}
			sb.AppendLine("</details>");
		} else {
			sb.AppendLine($"<div class='tree-item {color}' data-content='{safeContent}' data-type='{safeType}' data-line='{element.LineNumber}'><b>　　{HttpUtility.HtmlEncode(element.TypeName)}{field}{len}</b></div>");
		}
		return sb.ToString();
	}
	public static string ToHtmlTree(SymbolMatch element)
	{
		if (element == null)
			return "";
		var sb = new StringBuilder();
		if (element.CaptureType == "root") {
			foreach (var child in element.Childs) {
				sb.AppendLine(ToHtmlTree(child));
			}
		} else {
			string safeContent = System.Web.HttpUtility.HtmlEncode(element.Node.Content);
			string len = $"　<span class='range'>[{element.Node.StartByte}～{element.Node.EndByte}] [{element.Node.LineNumber}]</span>";
			string field = "";
			if (!string.IsNullOrEmpty(element.Declaration)) {
				field = $"<span class='field'>({element.Declaration})</span>";
			}
			string color = "";

			if (element.Childs.Count > 0) {
				sb.AppendLine("<details style='margin-left: 15px;'>");

				sb.AppendLine($"  <summary><span class='tree-item {color}' data-content='{safeContent}' data-line='{element.Node.LineNumber}'><b>{element.Name}({element.CaptureType}){len}</b></span></summary>");
				foreach (var child in element.Childs) {
					sb.AppendLine(ToHtmlTree(child));
				}
				sb.AppendLine("</details>");
			} else {
				sb.AppendLine($"<div class='tree-item {color}' data-content='{safeContent}' data-line='{element.Node.LineNumber}'><b>　　{element.Name}({element.CaptureType}){len}</b></div>");
			}
		}
		return sb.ToString();
	}

	public static string RenderOutline(LanguageDefinition langDef, SymbolMatch node)
	{
		StringBuilder callrefsb = new StringBuilder();
		StringBuilder callsb = new StringBuilder();
		StringBuilder refsb = new StringBuilder();

		var visibleCallInfos = node.CallInfos.Where(x => x.Visible).ToList();

		if (visibleCallInfos.Count > 0) {
			foreach (var item in visibleCallInfos) {
				string dec = "";
				if (item.ObjName != null) {
					dec += item.ObjName;
					if (item.IsOptionalObjectAccess) {
						dec += "?.";
					} else {
						dec += ".";
					}
				}

				dec += item.Name;
				if (item.IsOptionalCall) {
					dec += "?.";
				}
				string pre = "R";
				if (item.Type == "call.method" || item.Type == "call.function") {
					dec += "()";
					pre = "C";
				}
				string cname = item.Type.Replace(".", "-");
				string div_string = 
					$@"<div class='tree-item call'>
						<span class='name call {cname}' title='{System.Web.HttpUtility.HtmlEncode(dec)}'  data-sb='{item.StartByte}' data-eb='{item.EndByte}' data-def='{item.Name}'>{dec}</span><spam class='def-ln'>[{item.Node?.LineNumber.ToString() ?? "-"}]</spam>
					</div>"
						;
				callrefsb.AppendLine(div_string);
				if (item.Type == "call.method" || item.Type == "call.function") {
					callsb.AppendLine(div_string);
				} else {
					refsb.AppendLine(div_string);
				}
			}
		}

		// root自体はコンテナのみ、またはスキップして子要素から処理
		if (node.CaptureType == "root") {
			string toplevel = "";
			if (visibleCallInfos.Count > 0) {
				toplevel = $@"
				<details class=''>
					<summary>
						<span class='name' title='[Toplevel calls/refs]' data-sb='{node.StartByte}' data-eb='{node.EndByte}'>[Toplevel calls/refs]</span>
					</summary>
					<div class='tree-children'>
						{callsb.ToString()}
						{refsb.ToString()}
					</div>
				</details>";
			}
			return toplevel + string.Join("", node.Childs.Select(x => RenderOutline(langDef, x)));
		}

		string calls = "";
		if (visibleCallInfos.Count > 0) {
			calls = $@"
				<details class=''>
					<summary>
						<span class='name' title='[calls/refs]' data-sb='{node.StartByte}' data-eb='{node.EndByte}'>[calls/refs]</span><spam class='def-ln'>[{node.Node?.LineNumber.ToString() ?? "-"}]</spam>
					</summary>
					<div class='tree-children'>
						{callrefsb.ToString()}
					</div>
				</details>";
		}

		bool hasChildren = node.Childs != null && node.Childs.Count > 0;
		string accessClass = node.IsPublic ? "access-public" : "access-private";

		string capType = node.CaptureType.Replace(".", "-");
		//if (capType.StartsWith("definition") == false) {
		//	return "";
		//}


		if (hasChildren) {
			// 子がある場合は details / summary で組む
			// var childrenHtml = string.Join("", node.Childs.Select(RenderOutline));
			var childrenHtml = string.Join("", node.Childs.Select(x => RenderOutline(langDef, x)));

			string innerHtml = langDef.RenderCallsFirst
				? calls + childrenHtml
				: childrenHtml + calls;

			return $@"
        <details class='{accessClass}'>
            <summary>
                <span class='name {capType}' title='{System.Web.HttpUtility.HtmlEncode(node.Declaration)}' data-sb='{node.StartByte}' data-eb='{node.EndByte}'>{node.Name}</span><spam class='def-ln'>[{node.Node?.LineNumber.ToString() ?? "-"}]</spam>
			</summary>
            <div class='tree-children'>
                {innerHtml}
            </div>
        </details>";
		} else {
			if (visibleCallInfos.Count > 0) {

				return $@"
        <details class='{accessClass}'>
            <summary>
                <span class='name {capType}' title='{System.Web.HttpUtility.HtmlEncode(node.Declaration)}' data-sb='{node.StartByte}' data-eb='{node.EndByte}'>{node.Name}</span><spam class='def-ln'>[{node.Node?.LineNumber.ToString() ?? "-"}]</spam>
            </summary>
            <div class='tree-children'>
				{calls}
            </div>
        </details>";
			} else {
				// 末端ノード
				return $@"
					 <div class='tree-item {accessClass}'>
					    <span class='name {capType} ' title='{System.Web.HttpUtility.HtmlEncode(node.Declaration)}'  data-sb='{node.StartByte}' data-eb='{node.EndByte}'>{node.Name}</span><spam class='def-ln'>[{node.Node?.LineNumber.ToString() ?? "-"}]</spam>
					</div>";
			}
		}
	}




	public static List<SymbolMatch> CSharpParse(LanguageDefinition lang, CodeAnalysisLangResult result)
	{
		// regionをtree構造に組み込む 
		// やってみたけど、アウトライン表示の場合にはregionがあると目的地までのクリック数が1つ増えるぐらいの意味しか無い。
		// そもそもアウトラインだから目的地まで直ぐだし。。。一応コードは残しておく
		void InsertRegion(TreeSitterElement node) {

			if (node.Childs == null) {
				return;
			}
			var stack = new Stack<int>();
			var pairs = new List<(int Start, int End)>();
			for (int i = 0; i < node.Childs.Count; i++) {

				if (node.Childs[ i ].TypeName == "preproc_region") {
					stack.Push(i);
				} else if (node.Childs[ i ].TypeName == "preproc_endregion") {
					if (stack.Count == 0)
						continue;
					pairs.Add((stack.Pop(), i));
				}
			}
			if (pairs.Count > 0) {
				bool need_break = true;
			}
			pairs = pairs
				.OrderBy(x => x.End - x.Start)
				.ThenByDescending(x => x.Start)
				.ToList();
			var removeIndexes = new HashSet<int>();
			for (int i = 0; i < pairs.Count; i++) {
				var (start, end) = pairs[ i ];

				var regionNode = node.Childs[ start ];
				regionNode.TypeName = "region_area";

				for (int j = start + 1; j < end; j++) {
					var child = node.Childs[ j ];

					if (child.Papa == node && child.TypeName != "preproc_endregion") {
						// papa != nodeの場合にはネストして既に親が変わっている。 
						child.Papa = regionNode;
						regionNode.Childs.Add(child);
					}
				}
				for (int k = start + 1; k <= end; k++) {
					removeIndexes.Add(k);
				}
			}
			foreach (var idx in removeIndexes.OrderByDescending(x => x)) {
				node.Childs.RemoveAt(idx);
			}

			for (int i = 0; i < node.Childs.Count; i++) {
				InsertRegion(node.Childs[ i ]);
			}

			return;
		}
		// InsertRegion(result.ElementRoot);

		var list = CSharpSymbolExtractor.ExtractSymbol(lang.Id, result.ElementRoot, result.Root);






		//bool debug = false;
		//if (debug) {
		//	string temp_html = MemoUtil.ElementsTemplateHtml;
		//	string ele_detail = TreeSitterUtil.ToHtmlTree(result.Root);
		//	temp_html = temp_html.Replace("/*** replace detail tree by C# ***/", ele_detail);
		//	temp_html = temp_html.Replace("/*** replace source code ***/", System.Web.HttpUtility.HtmlEncode(body));
		//	using (var wr = new StreamWriter("symbols.html")) {
		//		wr.Write(temp_html);
		//	}

		//}
		//if (debug) {
		//	using (var wr = new StreamWriter("tags.txt")) {
		//		foreach (var item in list) {
		//			wr.WriteLine("-----------------------------");
		//			wr.WriteLine($"{item.Name} @{item.CaptureType} {item.Accessibility} {(item.IsStatic ? "static" : "")}  full:{item.FullQualifierName}");
		//			wr.WriteLine(item.Node.Content);
		//		}
		//	}
		//}
		return list;
	}
	public static List<SymbolMatch> HtmlParse(LanguageDefinition lang, CodeAnalysisLangResult result)
	{

		var list = HtmlSymbolExtractor.ExtractSymbol(lang.Id, result.ElementRoot, result.Root);


		return list;
	}

	// JavascriptParse 
	public static List<SymbolMatch> CssParse(LanguageDefinition lang, CodeAnalysisLangResult result)
	{
		var list = CssSymbolExtractor.ExtractSymbol(lang.Id, result.ElementRoot, result.Root);

		return list;
	}

	public static List<SymbolMatch> JavascriptParse(LanguageDefinition lang, CodeAnalysisLangResult result)
	{
		var list = JavascriptSymbolExtractor.ExtractSymbol(lang.Id, result.ElementRoot, result.Root);

		return list;
	}

	public static List<CallInfo> CollectCallInfos(List<SymbolMatch> list, SymbolMatch root)
	{
		var result = new List<CallInfo>();
		if (root?.CallInfos.Count > 0) {
			result.AddRange(root.CallInfos);
		}
		if (list == null) {
			return result;
		}
		foreach (var sym in list) {
			result.AddRange(sym.CallInfos);
		}
		return result;
	}

}
public static class TreeSitterStatic
{
	public static TreeSitterElement First(this TreeSitterElement node, string type)
	{
		return node?.Childs.FirstOrDefault(c => c.TypeName == type);
	}
	public static IEnumerable<TreeSitterElement> All(this TreeSitterElement node, string type)
	{
		return node?.Childs.Where(c => c.TypeName == type);
	}

	public static TreeSitterElement FirstField(this TreeSitterElement node, string type) {
		return node?.Childs.FirstOrDefault(c => c.FieldName == type);
	}
}





public class SymbolMatch
{
	public string Name {
		get; set;
	}        // 抽出された名前。識別子
	[JsonIgnore]
	public string CaptureType {
		get; set;
	} // definition.class, reference.class など

	[JsonIgnore]
	public string FullQualifierName {
		get; set;
	} // 名前空間等 

	public LanguageId Language { get; set; } = LanguageId.Non;

	// 外部ファイルからアクセスできるかどうか。言語非依存 
	public bool IsPublic { get; set; }

	[JsonIgnore]
	// 言語依存 
	public string Accessibility {
		get; set;
	}

	[JsonIgnore]
	// 静的かどうか。 
	public bool IsStatic { get; set; } = false;

	/// <summary>
	/// 
	/// </summary>
	// ユーザー表示用・外部検索での視認用 
	// C#  : "[Serializable] public partial class UserManager : IDisposable"
	//     : "[HttpGet] public async Task<User> GetUser(int id)"
	// TS  : "export async function getUser(id: number): Promise<User>"
	// Python: "def get_user(id: int) -> User:"
	public string Declaration { get; set; } = string.Empty;


	// この辺の[JsonIgnore]は、loopしてしまうのでそれを防止するため。 
	[JsonIgnore]
	public SymbolMatch Papa { get; set; } = null;
	[JsonIgnore]
	public List<SymbolMatch> Childs { get; set; } = new List<SymbolMatch>();

	[JsonIgnore]
	public TreeSitterElement Node {
		get; set;
	} // 対象となったノード. StartByteとEndByteを主に使う。

	public int StartByte { get; set; } = 0;
	public int EndByte { get; set; } = 0;

	public string FileId { get; set; } = string.Empty;
	[JsonIgnore]
	public List<CallInfo> CallInfos { get; set; } = new List<CallInfo>();
	[JsonIgnore]
	public List<CallInfo> PendingCallInfos { get; set; } = new List<CallInfo>();
}

public class CallInfo
{
	public string Name {get; set; }
	public string ObjName { get; set; }
	[JsonIgnore]
	public string Symbol {get;set;}
	public LanguageId Language { get; set; } = LanguageId.Non;

	[JsonIgnore]
	public string Type { get; set; }
	[JsonIgnore]
	// obj?.foo()の?. 
	public bool IsOptionalObjectAccess { get; set; }
	[JsonIgnore]
	// obj.foo?.()の?. 
	public bool IsOptionalCall {
		get; set;
	}
	[JsonIgnore]
	// アウトライン構造のtreeに入れるか 
	public bool Visible {
		get; set;
	} = true;

	[JsonIgnore]
	public TreeSitterElement Node {
		get; set;
	}
	public int StartByte { get; set; } = 0;
	public int EndByte { get; set; } = 0;

	public string UsageContext { get; set; }		// 前後の文脈 

	public string FileId { get; set; } = string.Empty;
}



public static class CommonSymbolType
{
	/*
	    definition.type			 (型)
	    definition.alias		（別名）
		definition.module		（名前空間、パッケージ）
		definition.class		（クラス、レコード）
		definition.interface	（インターフェース）
		definition.struct		（構造体）
		definition.enum			（列挙型）
		definition.enum_member	（列挙型のメンバー）
		definition.delegate		（デリゲート、関数の型定義） ★追加
		definition.method		（メソッド、関数）
		definition.property		（プロパティ、ゲッター/セッター）
		definition.event		（イベント、リスナー） ★追加
		definition.field		（フィールド、定数、変数）
		definition.element		（HTML等のid付き要素）
		definition.section		（Markdown等の見出し）
	 */

}



public static class LangParserUtil
{
	public static bool IsDefinedIdentifier(List<SymbolMatch> symbol_list, CallInfo cinfo)
	{
		if (symbol_list.Any(x => x.Name == cinfo.Name)) {
			return true;
		}
		if (cinfo.Name.Length > 4) {
			return true;
		}
		return false;
	}

	public static void ResolvePendingCallInfos(List<SymbolMatch> symbol_list)
	{
		foreach (var symbol in symbol_list) {
			foreach (var cinfo in symbol.PendingCallInfos) {

				if (!IsDefinedIdentifier(symbol_list, cinfo))
					continue;
				int index = symbol.CallInfos.FindIndex(
					x => x.Node.StartByte > cinfo.Node.StartByte
				);

				if (index < 0) {
					symbol.CallInfos.Add(cinfo);
				} else {
					symbol.CallInfos.Insert(index, cinfo);
				}
			}
			symbol.PendingCallInfos.Clear();
		}
	}
}




public static class CSharpSymbolExtractor
{

	// 1. C#のノード名から、標準化された一貫性のあるカテゴリ名へのマッピングを用意
	private static readonly Dictionary<string, string> CSharpTypeMap = new Dictionary<string, string>
	{
		// using_directiveには2つ意味があり得る  
		//{ "using_directive",          "type"},
		//{ "using_directive",          "alias"},
		{ "namespace_declaration",   "module" },
		{ "file_scoped_namespace_declaration", "module"},
		{ "class_declaration",       "class" },
		{ "interface_declaration",   "interface" },
		{ "struct_declaration",      "struct" },
		{ "record_declaration",      "class" },
		{ "enum_declaration",        "enum" },
		{ "enum_member_declaration", "enum_member"},
		{ "constructor_declaration", "method" },
		{ "destructor_declaration",  "method" },
		{ "method_declaration",      "method" },
		{ "property_declaration",    "property" },
		{ "event_declaration",       "event" },
		{ "delegate_declaration",    "delegate" },
		{ "field_declaration",       "field"},
		{ "event_field_declaration", "field"},
	};

	public static List<SymbolMatch> ExtractSymbol(LanguageId langName, TreeSitterElement root, SymbolMatch sym_root)
	{
		var results = new List<SymbolMatch>();
		ExtractSymbolRecursive(langName, root, results, sym_root);

		// トップレベルの奴はusing系なので無駄 
		sym_root.PendingCallInfos.Clear();
		//ResolvePendingCallInfos(sym_root, results);
		LangParserUtil.ResolvePendingCallInfos(results);
		return results;
	}
	public static bool IsDefinedIdentifier(List<SymbolMatch> symbol_list, CallInfo cinfo)
	{
		return symbol_list.Any(x => x.Name == cinfo.Name);
	}

	public static void ResolvePendingCallInfos(SymbolMatch root, List<SymbolMatch> symbol_list)
	{
		if (root == null)
			return;

		ResolvePendingCallInfosRecursive(root, symbol_list);
	}

	private static void ResolvePendingCallInfosRecursive(SymbolMatch symbol, List<SymbolMatch> symbol_list)
	{
		// PendingCallInfos を処理
		if (symbol.PendingCallInfos != null && symbol.PendingCallInfos.Count > 0) {
			var existingStartBytes = new HashSet<int>(
				symbol.CallInfos.Select(x => x.Node.StartByte)
			);


			foreach (var cinfo in symbol.PendingCallInfos) {
				if (!IsDefinedIdentifier(symbol_list, cinfo))
					continue;

				int index = symbol.CallInfos.FindIndex(
					x => x.Node.StartByte > cinfo.Node.StartByte
				);

				if (index < 0) {
					symbol.CallInfos.Add(cinfo);
				} else {
					symbol.CallInfos.Insert(index, cinfo);
				}
			}
			symbol.PendingCallInfos.Clear();
		}
		
		// 子を再帰的に処理
		if (symbol.Childs == null)
			return;

		foreach (var child in symbol.Childs) {
			ResolvePendingCallInfosRecursive(child, symbol_list);
		}
	}



	private static void ExtractSymbolRecursive(LanguageId langName, TreeSitterElement node, List<SymbolMatch> results, SymbolMatch sym_papa)
	{
		if (node == null)
			return;

		SymbolMatch sym_node = sym_papa;


		// ------------------------------------------------------------------
		// 別名
		// ------------------------------------------------------------------
		if (node.TypeName == "using_directive") {
			var nameNode = node.Childs.FirstOrDefault(c => c.FieldName == "name");
			if (nameNode != null) {
				bool hasGlobal = node.Childs.Any(c => c.Content == "global");
				bool hasTuple = node.Childs.Any(c => c.TypeName == "tuple_type");


				sym_node = new SymbolMatch {
					Name = nameNode.Content,
					CaptureType = hasTuple ? "definition.type" : "definition.alias",
					FullQualifierName = GetFullQualifierName(node),
					Language = langName,
					Accessibility = hasGlobal ? "global" : "private",
					// IsPublic = hasGlobal,
					IsPublic = true,        // publicな識別子がローカルなaliasを使用してる可能性があるので 
					IsStatic = node.Childs.Any(c => c.Content == "static"),
					Declaration = GetDeclaration(node),
					Node = node, // declarationノード自体を設定
					Papa = sym_papa
				};
				sym_papa.Childs.Add(sym_node);
				results.Add(sym_node);
			}
		}
		// 特殊。これは自前で挿入している 
		else if (node.TypeName == "region_area") {
			var nameNode = node.Childs.FirstOrDefault(c => c.FieldName == "content");
			if (nameNode != null) {
				sym_node = new SymbolMatch {
					Name = nameNode.Content,
					//CaptureType = $"definition.{suffix}",
					CaptureType = $"definition.region",
					FullQualifierName = "",
					Language = langName,
					Accessibility = "none",
					IsPublic = false,
					IsStatic = false,
					Declaration = "#region " + nameNode.Content,
					Node = node, // declarationノード自体を設定
					Papa = sym_papa
				};
				sym_papa.Childs.Add(sym_node);
			}
		}
		  // ------------------------------------------------------------------
		  // A. 【定義系 (definition.***)】のハント
		  // ------------------------------------------------------------------
		  else if (node.TypeName == "namespace_declaration" ||
			  node.TypeName == "file_scoped_namespace_declaration" ||
			  node.TypeName == "class_declaration" ||
			  node.TypeName == "constructor_declaration" ||
			  node.TypeName == "destructor_declaration" ||
			  node.TypeName == "interface_declaration" ||
			  node.TypeName == "struct_declaration" ||
			  node.TypeName == "record_declaration" ||
			  node.TypeName == "delegate_declaration" ||
			  node.TypeName == "property_declaration" ||
			  node.TypeName == "event_declaration" ||
			  node.TypeName == "method_declaration") {
			var nameNode = node.Childs.FirstOrDefault(c => c.FieldName == "name");
			if (nameNode != null) {
				string suffix = node.TypeName.Replace("_declaration", "");
				if (suffix == "namespace" || suffix == "file_scoped_namespace")
					suffix = "module";

				string name = nameNode.Content;
				if (node.TypeName == "destructor_declaration") {
					name = "~" + name;
				}
				// CSharpTypeMap 
				sym_node = new SymbolMatch {
					Name = name,
					//CaptureType = $"definition.{suffix}",
					CaptureType = $"definition.{CSharpTypeMap[node.TypeName]}",
					FullQualifierName = GetFullQualifierName(node),
					Language = langName,
					Accessibility = GetAccessibility(node),
					IsPublic = GetIsPublic(node),
					IsStatic = node.Childs.Any(c => c.Content == "static"),
					Declaration = GetDeclaration(node),
					Node = node, // declarationノード自体を設定
					Papa = sym_papa
				};
				sym_papa.Childs.Add(sym_node);
				results.Add(sym_node);

			}
		}
		  // ★【特殊処理】enum とその中身（メンバー）をハント
		  else if (node.TypeName == "enum_declaration") {
			// ① enum自体の定義を登録
			var nameNode = node.Childs.FirstOrDefault(c => c.FieldName == "name");
			if (nameNode != null) {
				sym_node = new SymbolMatch {
					Name = nameNode.Content,
					CaptureType = "definition.enum",
					FullQualifierName = GetFullQualifierName(node),
					Language = langName,
					Accessibility = GetAccessibility(node),
					IsPublic = GetIsPublic(node),
					IsStatic = false, // enum自体にstatic修飾子は付かない
					Declaration = GetDeclaration(node),
					Node = node,
					Papa = sym_papa
				};
				sym_papa.Childs.Add(sym_node);
				results.Add(sym_node);
			}

			// ② enumの中身（メンバー識別子）を掘り起こす
			ExtractEnumMembers(langName, node, node, results, sym_node);
		}
		  // ★【修正版】フィールド定義（1枚挟まった階層構造に対応）
		  else if (node.TypeName == "field_declaration" || node.TypeName == "event_field_declaration") {
			// 直下にある variable_declaration ノードを探す
			var varDeclNode = node.Childs.FirstOrDefault(c => c.TypeName == "variable_declaration");

			if (varDeclNode != null) {
				// variable_declaration の下にあるすべての変数宣言（名前部分）をループ
				var declarators = varDeclNode.Childs.Where(c => c.TypeName == "variable_declarator");
				foreach (var decl in declarators) {
					var nameNode = decl.Childs.FirstOrDefault(c => c.FieldName == "name");
					if (nameNode != null) {
						sym_node = new SymbolMatch {
							Name = nameNode.Content,
							CaptureType = "definition.field",
							FullQualifierName = GetFullQualifierName(node),
							Language = langName,
							Accessibility = GetAccessibility(node), // アクセス権は一番親の field_declaration から取る
							IsPublic = GetIsPublic(node),
							IsStatic = node.Childs.Any(c => c.Content == "static"),
							Declaration = GetDeclaration(node),
							Node = node, // field_declarationノードを設定
							Papa = sym_papa
						};
						sym_papa.Childs.Add(sym_node);
						results.Add(sym_node);
					}
				}
			}
		}

		  // ------------------------------------------------------------------
		  // B. 【参照系 (reference.***)】のハント
		  //     現状は使用しない
		  // ------------------------------------------------------------------
		  else if (node.TypeName == "variable_declaration") {
			//// フィールド（field_declaration）の子である variable_declaration は上の A グループで
			//// 完全に回収済みのため、ここでは純粋に「ローカル変数の型宣言」だけを拾って重複を防ぐ
			//bool isLocalVariable = node.Papa?.TypeName == "local_declaration_statement";

			//if (isLocalVariable) {
			//	var typeNode = node.Childs.FirstOrDefault(c => c.FieldName == "type");
			//	if (typeNode != null && IsTargetReference(typeNode.Content)) {
			//		sym_node = new SymbolMatch {
			//			Name = typeNode.Content,
			//			CaptureType = "reference.class",
			//			FullQualifierName = null,
			//			Language = langName,
			//			Accessibility = "",
			//			IsStatic = false,
			//			Node = node,
			//			Papa = sym_papa
			//		};
			//		sym_papa.Childs.Add(sym_node);
			//		results.Add(sym_node);
			//	}
			//}
		} else if (node.TypeName == "object_creation_expression" ) {
			// new Padding(10)とかの部分 
			var typeNode = node.FirstField("type");
			if (typeNode.TypeName == "identifier") {
				sym_papa.CallInfos.Add(new CallInfo {
					Name = typeNode.Content,
					Symbol = typeNode.Content,
					Language = langName,
					Type = "call.method",			// コンストラクタを呼び出すという意味でcall 
					Node = node
				});
			}


		} else if (node.TypeName == "base_list") {
			// class Child : Parent, IInterfaceA, IInterfaceB の:以下の部分がbase_list 
			foreach (var baseType in node.Childs.Where(c => c.IsNamed)) {
				string baseName = ""; 
				if (baseType.TypeName == "identifier") {
					baseName = baseType.Content;
				} else if (baseType.TypeName == "generic_name") {
					baseName = baseType.First("identifier")?.Content;
				}
				if (!string.IsNullOrEmpty(baseName)) {
					sym_papa.CallInfos.Add(new CallInfo {
						Name = baseName,
						Symbol = baseName,
						Language = langName,
						Type = "ref.symbol",
						Visible = false,
						Node = node
					});
				}
			}

		} else if (node.TypeName == "invocation_expression") {
			var funcNode = node.FirstField("function");
			if (funcNode?.TypeName == "identifier") {
				// Foo(); 
				sym_papa.CallInfos.Add(new CallInfo {
					Name = funcNode.Content,
					Symbol = funcNode.Content,
					Language = langName,
					Type = "call.method",
					Node = node
				});
			}
		} else if (node.TypeName == "member_access_expression") {
			var funcNode = node.FirstField("name");
			if (funcNode?.TypeName == "identifier") {
				// Console.WriteLine("hello")
				if (node.Papa?.FirstField("arguments") != null) {
					// foo.boo(); 
					//if ("MapLogEndpoints" == funcNode.Content) {
					//	bool need_break = true;
					//}

					sym_papa.CallInfos.Add(new CallInfo {
						Name = funcNode.Content,
						Symbol = funcNode.Content,
						Language = langName,
						Type = "call.method",
						Node = node
					});
				} else {
					// foo.boo; 
					sym_papa.CallInfos.Add(new CallInfo {
						Name = funcNode.Content,
						Symbol = funcNode.Content,
						Language = langName,
						Type = "ref.symbol",
						Visible = false,
						Node = node
					});
				}
			}
		} else if (node.TypeName == "identifier" && (node.FieldName != "name" && node.FieldName != "function")) {
			sym_papa.PendingCallInfos.Add(new CallInfo {
				Name = node.Content,
				Symbol = node.Content,
				Language = langName,
				Type = "ref.symbol",
				Visible = false,
				Node = node
			});
		}

		// ------------------------------------------------------------------
		// 子ノードたちに対して再帰的にループ
		// （※enum_declarationの直下の子も、通常の再帰ループに巻き込まないよう
		//  特殊処理側で処理を完結させているため、重複せず安全です）
		// ------------------------------------------------------------------
		foreach (var child in node.Childs) {
			ExtractSymbolRecursive(langName, child, results, sym_node);
		}
	}

	/// <summary>
	/// enumの配下からメンバー（Red, Greenなど）を安全に掘り起こすヘルパー
	/// </summary>
	private static void ExtractEnumMembers(LanguageId langName, TreeSitterElement enum_node, TreeSitterElement node, List<SymbolMatch> results, SymbolMatch sym_papa)
	{
		foreach (var child in node.Childs) {
			if (child.TypeName == "enum_member_declaration") {
				var memberNameNode = child.Childs.FirstOrDefault(c => c.FieldName == "name" && c.TypeName == "identifier");
				if (memberNameNode != null) {
					var sym_node =  new SymbolMatch {
						Name = memberNameNode.Content,
						CaptureType = "definition.enum_member",
						FullQualifierName = GetFullQualifierName(node),
						Language = langName,
						Accessibility = "public", // enumのメンバーは常に外部公開なので実質public
						IsPublic = GetIsPublic(node),
						IsStatic = true,         // enumのメンバーは実質staticのように扱える
						Declaration = enum_node.Content,
						Node = child,             // ★enum_member_declarationノードを設定！
						Papa = sym_papa
					};
					sym_papa.Childs.Add(sym_node);
					results.Add(sym_node);
				}
			} else {
				// enum_member_declaration_list などの階層をさらに深く掘る
				ExtractEnumMembers(langName, enum_node, child, results, sym_papa);
			}
		}
	}

	private static string GetAccessibility(TreeSitterElement element)
	{
		if (element.TypeName == "namespace_declaration" || element.TypeName == "file_scoped_namespace_declaration") {
			return "public";
		}

		bool hasPublic = element.Childs.Any(c => c.Content == "public");
		bool hasInternal = element.Childs.Any(c => c.Content == "internal");
		bool hasProtected = element.Childs.Any(c => c.Content == "protected");
		bool hasPrivate = element.Childs.Any(c => c.Content == "private");

		if (hasPublic)
			return "public";
		if (hasProtected && hasInternal)
			return "protected internal";
		if (hasPrivate && hasProtected)
			return "private protected";
		if (hasProtected)
			return "protected";
		if (hasPrivate)
			return "private";
		if (hasInternal)
			return "internal";

		if (element.Papa?.TypeName == "compilation_unit" || element.Papa?.TypeName == "namespace_declaration" || element.Papa?.TypeName == "file_scoped_namespace_declaration") {
			return "internal";
		}
		return "private";
	}
	private static bool GetIsPublic(TreeSitterElement element)
	{
		string accessibility = GetAccessibility(element);

		// 同一プロジェクト内の別ファイルからアクセス可能なものはすべて true
		return accessibility == "public" ||
			   accessibility == "internal" ||
			   accessibility == "protected internal";
	}
	static string GetDeclaration(TreeSitterElement node)
	{
		if (node.TypeName == "enum_declaration") {
			return node.Content;
		}

		StringBuilder sb  =new StringBuilder();

		foreach (var child in node.Childs) {
			if (child.TypeName == "declaration_list" || child.TypeName == "accessor_list" || child.TypeName == "block") {
				break;
			}
			sb.Append($" {child.Content}");
		}



		// Declaration 
		// declaration 
		return sb.ToString();
	}



	private static bool IsTargetReference(string name)
	{
		string[] noise = { "var", "int", "string", "bool", "double", "float", "long", "void", "object", "char", "byte" };
		return !noise.Contains(name);
	}


	public static string GetFullQualifierName(TreeSitterElement node)
	{
		var pathParts = new List<string>();

		// 現在のノードから、親（Papa）をたどって最上位までループ
		TreeSitterElement current = node;

		current = node.Papa;

		while (current != null) {
			// 親（または自分自身）が名前を持つ定義ノードの場合、その名前をリストに加える
			if (current.TypeName == "class_declaration" ||
				current.TypeName == "interface_declaration" ||
				current.TypeName == "struct_declaration" ||
				current.TypeName == "record_declaration" ||
				current.TypeName == "enum_declaration" ||
				current.TypeName == "namespace_declaration" || 
				current.TypeName == "file_scoped_namespace_declaration") {
				//if (current.TypeName == "namespace_declaration" ||
				//	current.TypeName == "class_declaration" ||
				//	current.TypeName == "constructor_declaration" ||
				//	current.TypeName == "interface_declaration" ||
				//	current.TypeName == "struct_declaration" ||
				//	current.TypeName == "record_declaration" ||
				//	current.TypeName == "delegate_declaration" ||
				//	current.TypeName == "property_declaration" ||
				//	current.TypeName == "event_declaration") {
				var nameNode = current.Childs.FirstOrDefault(c => c.FieldName == "name");
				if (nameNode != null) {
					// 下から順（Inner -> Outer -> Namespace）に名前を挿入していく
					pathParts.Insert(0, nameNode.Content);
				}
			}

			// さらに上の親（Papa）へ進む
			current = current.Papa;
		}

		// すべての名前をドット「.」で連結する
		// 例: ["MyNamespace", "OuterClass", "InnerClass"] ➡️ "MyNamespace.OuterClass.InnerClass"
		return string.Join(".", pathParts);
	}


	/// <summary>
	/// 指定されたメソッドノード内でアクセス（生成・宣言・静的呼出）されているクラスのランキングを集計します。
	/// </summary>
	public static Dictionary<string, int> GetClassAccessRanking(TreeSitterElement methodNode)
	{
		var ranking = new Dictionary<string, int>();

		// メソッドのボディ（波括弧 { } で囲まれた block ノード）を探す
		var bodyNode = methodNode.Childs.FirstOrDefault(c => c.TypeName == "block");
		if (bodyNode == null)
			return ranking;

		// メソッドの中身を再帰的にスキャンしてカウント
		CountClassReferences(bodyNode, ranking);

		// 登場回数が多い順（降順）にソートして辞書として返す
		return ranking.OrderByDescending(pair => pair.Value)
					  .ToDictionary(pair => pair.Key, pair => pair.Value);
	}

	private static void CountClassReferences(TreeSitterElement node, Dictionary<string, int> ranking)
	{
		if (node == null)
			return;

		string className = null;

		// ------------------------------------------------------------------
		// パターン1: new HttpClient() [オブジェクト生成]
		// ------------------------------------------------------------------
		if (node.TypeName == "object_creation_expression") {
			// type フィールドの子ノード（型名）を一本釣り
			var typeNode = node.Childs.FirstOrDefault(c => c.FieldName == "type");
			if (typeNode != null) {
				className = typeNode.Content;
			}
		}
		// ------------------------------------------------------------------
		// パターン2: HttpClient client = ... [明示的な変数宣言]
		// ------------------------------------------------------------------
		else if (node.TypeName == "variable_declaration") {
			// type フィールドの子ノード（型名）を一本釣り
			var typeNode = node.Childs.FirstOrDefault(c => c.FieldName == "type");
			if (typeNode != null) {
				className = typeNode.Content;
			}
		}
		// ------------------------------------------------------------------
		// パターン3: MyClass.DoSomething() [静的メソッド呼び出し・プロパティアクセス]
		// ------------------------------------------------------------------
		else if (node.TypeName == "member_access_expression") {
			// ドットの左側（expression フィールド）を取得
			var expressionNode = node.Childs.FirstOrDefault(c => c.FieldName == "expression");

			// 左側が単なる「識別子（identifier）」かつ、先頭が大文字（クラス名っぽい）ならハント
			// （変数名 client.格式 などを除外するための簡易判定ルール）
			if (expressionNode != null && expressionNode.TypeName == "identifier") {
				if (char.IsUpper(expressionNode.Content[0])) {
					className = expressionNode.Content;
				}
			}
		}

		// ------------------------------------------------------------------
		// ランキングへの登録とノイズフィルター
		// ------------------------------------------------------------------
		if (!string.IsNullOrEmpty(className)) {
			// C#の組み込み型や var はクラス名ランキングから除外する
			if (IsTargetClass(className)) {
				if (ranking.ContainsKey(className))
					ranking[className]++;
				else
					ranking[className] = 1;
			}
		}

		// 子ノードたちに対しても再帰的に同じスキャンを実行する
		foreach (var child in node.Childs) {
			CountClassReferences(child, ranking);
		}
	}

	/// <summary>
	/// ランキングの集計対象とする「クラス名」かどうかを判定するフィルター
	/// </summary>
	private static bool IsTargetClass(string name)
	{
		// 以下のキーワードはクラス名ではないので弾く
		string[] noiseKeywords = {
			"var", "int", "string", "bool", "double", "float",
			"long", "void", "object", "char", "byte", "decimal"
		};

		return !noiseKeywords.Contains(name);
	}
}




public static class HtmlSymbolExtractor
{
	public static List<SymbolMatch> ExtractSymbol(LanguageId langName, TreeSitterElement root, SymbolMatch sym_root)
	{
		var results = new List<SymbolMatch>();

		SymbolMatch alert_top = new SymbolMatch {
			Name = "[Alerts]",
			Language = langName,
			CaptureType = "alert",
			StartByte = -1,
			EndByte = -1,
			IsPublic = false,
			IsStatic = false,
			Papa = sym_root,
		};

		ExtractSymbolRecursive(langName, root, results, sym_root, alert_top);

		if (alert_top.Childs.Count > 0) {
			sym_root.Childs.Insert(0, alert_top);
		}

		return results;
	}

	private static void ExtractSymbolRecursive(LanguageId langName, TreeSitterElement node, List<SymbolMatch> results, 
		SymbolMatch sym_papa, SymbolMatch alert_top)
	{
		if (node == null)
			return;

		SymbolMatch sym_node = sym_papa;

		//if (node.TypeName == "element")

		var tag_ele = node.First("start_tag");
		if (tag_ele == null) {
			tag_ele = node.First("self_closing_tag");
		}

		if (tag_ele != null) {
			var tag_name = tag_ele.First("tag_name");
			if (tag_name == null) {
				return;
			}
			List<HtmlAttribute> attr_list = new List<HtmlAttribute>();
			var attributes = tag_ele.All("attribute");
			foreach (var attr in attributes) {
				var attr_name = attr.First("attribute_name");
				if (attr_name == null) {
					continue;
				}
				var attr_val = GetAttributeValue(attr);
				if (attr_val == null) {
					attr_list.Add(new HtmlAttribute {Name = attr_name.Content });
				} else {
					attr_list.Add(new HtmlAttribute { Name = attr_name.Content, Value = attr_val.Content });
				}
			}

			// 属性の重複はエラー 
			var duplicateGroups = attr_list
				.GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
				.Where(g => g.Count() > 1)
				.ToList();

			if (duplicateGroups.Count > 0) {
				string dup_names = string.Join(", ", duplicateGroups.Select(g => g.Key));

				alert_top.Childs.Add(new SymbolMatch {
					Name = $"属性が重複しています ({dup_names})",
					Language = langName,
					CaptureType = "warning",
					StartByte = node.StartByte,
					EndByte = node.EndByte,
					Node = node,
					Papa = alert_top
				});
			}
			List<CallInfo> callInfos = new List<CallInfo>();
			foreach (var attr in attr_list) {
				string jscode = "";
				if (attr.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase)) {
					jscode = attr.Value;
				}
				if (attr.Name.Equals("href", StringComparison.OrdinalIgnoreCase)
					&& attr.Value.IndexOf("javascript:", StringComparison.OrdinalIgnoreCase) != -1) {
					jscode = attr.Value;

				}
				if (!string.IsNullOrEmpty(jscode)) {
					if ((jscode.StartsWith("\"") && jscode.EndsWith("\"")) ||
						(jscode.StartsWith("'") && jscode.EndsWith("'"))) {
						jscode = jscode.Substring(1, jscode.Length - 2);
					}
					jscode = jscode.Replace("javascript:", "");
					var calls = ExtractCallNames(jscode);
					foreach (var call in calls) {
						callInfos.Add(new CallInfo {
							Name = call,
							Symbol = call,
							Type = "call.function",
							Node = node,
							IsOptionalCall = false,
							IsOptionalObjectAccess = false,
							Visible = false
						});
					}

				}
			}




			string dispname = tag_name.Content;
			var ids = GetAttribute(attr_list, "id");
			var classes = GetAttribute(attr_list, "class");
			var names = GetAttribute(attr_list, "name");
			bool done = false;


			if (ids.Count > 0) {
				// css用 
				callInfos.Add(new CallInfo {
					Name = "#" + ids.Last().Value,
					Symbol = "#" + ids.Last().Value,
					Type = "ref.symbol",
					Node = node,
					IsOptionalCall = false,
					IsOptionalObjectAccess = false,
					Visible = false
				});
			}
			if (classes.Count > 0) {
				foreach (var cls in classes.Last().Value.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)) {
					callInfos.Add(new CallInfo {
						Name = "." + cls,
						Symbol = "." + cls,
						Type = "ref.symbol",
						Node = node,
						IsOptionalCall = false,
						IsOptionalObjectAccess = false,
						Visible = false
					});
				}
			}


			if (!done && ids != null && ids.Count > 0) {
				dispname += "#" + ids.Last().Value;
				done = true;

				var id_node = new SymbolMatch {
					Name = ids.Last().Value,
					CaptureType = "definition.id",
					Language = langName,
					IsPublic = true,
					Declaration = tag_ele.Content,
					Node = tag_ele,
					CallInfos = callInfos,
					Papa = sym_papa
				};
				results.Add(id_node);
			} 
			if (!done && classes != null && classes.Count > 0) {
				var cnames = SplitClassNames(classes.Last().Value);
				if (cnames.Length > 0) {
					dispname += "." + cnames[0];
					done = true;
				}
			}
			if (!done && names != null && names.Count > 0) {
				dispname += "[name=" + names.Last().Value + "]";
				done = true;

				var id_node = new SymbolMatch {
					Name = names.Last().Value,
					CaptureType = "definition.name",
					Language = langName,
					IsPublic = true,
					Declaration = tag_ele.Content,
					Node = tag_ele,
					CallInfos = callInfos,
					Papa = sym_papa
				};
				results.Add(id_node);
			}

			string def_type = GetTagClass(tag_name.Content);
			sym_node = new SymbolMatch {
				Name = dispname,
				CaptureType = def_type,
				Language = langName,
				IsPublic = true,
				Declaration = tag_ele.Content,
				Node = tag_ele,
				CallInfos = callInfos,
				Papa = sym_papa
			};
			sym_papa.Childs.Add(sym_node);
			//results.Add(sym_node);
			// div[name=email] 
		}


		foreach (var child in node.Childs) {
			ExtractSymbolRecursive(langName, child, results, sym_node, alert_top);
		}
	}

	private static readonly Regex callRegex = new Regex(
		@"(?<![\w$])(?:new\s+)?(?:[\w$]+\.)*([\w$]+)\s*\(",
		RegexOptions.Compiled);

	public static List<string> ExtractCallNames(string jsCode)
	{
		var result = new List<string>();



		foreach (Match m in callRegex.Matches(jsCode)) {
			var name = m.Groups[1].Value;

			// JSの制御構文は除外
			switch (name) {
				case "if":
				case "for":
				case "while":
				case "switch":
				case "catch":
				case "function":
					continue;
			}

			result.Add(name);
		}

		return result.Distinct().ToList();
	}

	static string[] SplitClassNames(string classValue)
	{
		return classValue.Split(new[] { ' ', '\t', '\r', '\n', '\f' }, 	StringSplitOptions.RemoveEmptyEntries);
	}

	static List<HtmlAttribute> GetAttribute(List<HtmlAttribute> attr_list, string name)
	{
		return attr_list.FindAll(a => string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase));
		//List<HtmlAttribute> list = new List<HtmlAttribute>();
		//foreach (var attr in attr_list) {
		//	if (string.Equals(name, attr.Name, StringComparison.OrdinalIgnoreCase)) {
		//		list.Add(attr);
		//	}
		//}
		//return list;
	}

	class HtmlAttribute
	{
		public string Name;
		public string Value = null;
	}

	static TreeSitterElement GetAttributeValue(TreeSitterElement attr)
	{
		var v = attr.First("quoted_attribute_value")?.First("attribute_value");

		if (v != null)
			return v;

		return attr.First("attribute_value");
	}
	static string GetTagClass(string tag_name)
	{
		switch (tag_name.ToLower()) {
			// layout
			case "div":
			case "section":
			case "article":
			case "main":
			case "header":
			case "footer":
			case "aside":
			case "nav":
				return "definition.layout";

			// text
			case "h1":
			case "h2":
			case "h3":
			case "h4":
			case "h5":
			case "h6":
			case "p":
			case "span":
			case "strong":
			case "em":
				return "definition.text";

			// link
			case "a":
				return "definition.link";

			// form
			case "form":
			case "input":
			case "textarea":
			case "select":
			case "option":
			case "button":
			case "label":
				return "definition.form";

			// table
			case "table":
			case "thead":
			case "tbody":
			case "tr":
			case "td":
			case "th":
				return "definition.table";

			// media
			case "img":
			case "video":
			case "audio":
			case "canvas":
			case "svg":
				return "definition.media";

			case "style":
				return "definition.style";

			case "script":
				return "definition.script";

			// fallback
			default:
				return "definition.other";
		}
	}

}
public static class CssSymbolExtractor
{
	public static HashSet<string> Classes = new HashSet<string>();

	public static List<SymbolMatch> ExtractSymbol(LanguageId langName, TreeSitterElement root, SymbolMatch sym_root)
	{
		var results = new List<SymbolMatch>();
		ExtractSymbolRecursive(langName, root, results, sym_root);

		List<SymbolMatch> top_list = sym_root.Childs;

		var duplicateGroups = top_list
			.GroupBy(x => x.Name, StringComparer.Ordinal)
			.Where(g => g.Count() > 1)
			.ToList();
		if (duplicateGroups.Count > 0) {
			SymbolMatch alert_top = new SymbolMatch {
				Name = "[Alerts]",
				Language = langName,
				CaptureType = "alert",
				StartByte = -1,
				EndByte = -1,
				IsPublic = false,
				IsStatic = false,
				Papa = sym_root,
			};
			foreach (var item in duplicateGroups) {
				alert_top.Childs.Add(new SymbolMatch {
					Name = $"重複: {item.Key}",
					Language = langName,
					CaptureType = "warning",
					StartByte = item.ToList()[0].StartByte,
					EndByte = item.ToList()[0].EndByte,
					Node = item.ToList()[0].Node,
					Papa = alert_top
				});
			}
			sym_root.Childs.Insert(0, alert_top);
		}

		bool debug = false;
		if (debug) {
			foreach (var item in Classes) {
				Console.WriteLine(item);
			}
		}
		return results;
	}

	private static void ExtractSymbolRecursive(LanguageId langName, TreeSitterElement node, List<SymbolMatch> results, SymbolMatch sym_papa)
	{
		if (node == null)
			return;

		SymbolMatch sym_node = sym_papa;
		if (node.TypeName == "import_statement" ||
			node.TypeName == "namespace_statement" ||
			node.TypeName == "supports_statement" ||
			node.TypeName == "media_statement" ||
			node.TypeName == "scope_statement" ||
			node.TypeName == "keyframes_statement" ||
			node.TypeName == "charset_statement"
			) {
			string suffix = node.TypeName.Replace("_statement", "");
			string cname = $"definition.{suffix}";
			Classes.Add(cname);
			sym_node = new SymbolMatch {
				Name = GetDeclaration(node),
				CaptureType = cname,
				Language = langName,
				IsPublic = true,
				Declaration = GetDeclaration(node),
				Node = node,
				Papa = sym_papa
			};
			sym_papa.Childs.Add(sym_node);
			results.Add(sym_node);

			//
		} else if (node.TypeName == "at_rule") {
			var key = node.Childs.FirstOrDefault(c => c.TypeName == "at_keyword");
			if (key == null) {
				return;
			}
			string cname = $"definition.{key.Content.Replace("@", "")}";
			Classes.Add(cname);

			sym_node = new SymbolMatch {
				Name = GetDeclaration(node),
				CaptureType = cname,
				Language = langName,
				IsPublic = true,
				Declaration = GetDeclaration(node),
				Node = node,
				Papa = sym_papa
			};
			sym_papa.Childs.Add(sym_node);
			results.Add(sym_node);
		} else if (node.TypeName == "rule_set") {
			var sels = node.Childs.FirstOrDefault(c => c.TypeName == "selectors");
			if (sels == null) {
				return;
			}
			string cname = $"definition.selector";
			Classes.Add(cname);
			var sel_list = GetSelectors(sels);
			foreach (var item in sel_list) {
				sym_node = new SymbolMatch {
					Name = item.Text,
					CaptureType = cname,
					Language = langName,
					IsPublic = true,
					Declaration = item.Text,
					Node = item.Element,
					Papa = sym_papa
				};
				sym_papa.Childs.Add(sym_node);
				results.Add(sym_node);
			}
		}

		foreach (var child in node.Childs) {
			ExtractSymbolRecursive(langName, child, results, sym_node);
		}
	}

	static List<(string Text, TreeSitterElement Element)> GetSelectors(TreeSitterElement node)
	{
		List<(string Text, TreeSitterElement Element)> list = new List<(string Text, TreeSitterElement Element)>();
		foreach (var item in node.Childs) {
			if (item.Childs.Count == 0 && item.TypeName != "tag_name") {
				continue;
			}
			list.Add((item.Content, item));
		}
		return list;
	}

	static string GetDeclaration(TreeSitterElement node)
	{
		StringBuilder sb = new StringBuilder();

		foreach (var child in node.Childs) {
			if (child.TypeName == ";" || child.TypeName == "block" || child.TypeName.EndsWith("block_list")) {
				break;
			}
			sb.Append($" {child.Content}");
		}
		return sb.ToString();
	}

}
public static class JavascriptSymbolExtractor
{
	public static List<SymbolMatch> ExtractSymbol(LanguageId langName, TreeSitterElement root, SymbolMatch sym_root)
	{
		//CallGraphBuilder cb = new CallGraphBuilder();
		//cb.Build(root);
		//cb.PrintGraph();

		var results = new List<SymbolMatch>();
		ExtractSymbolRecursive(langName, root, results, sym_root);
		LangParserUtil.ResolvePendingCallInfos( results);
		return results;
	}



	private static void ExtractSymbolRecursive(LanguageId langName, TreeSitterElement node, List<SymbolMatch> results, SymbolMatch sym_papa)
	{
		if (node == null)
			return;

		//if ((node.TypeName == "statement_block" || node.TypeName.EndsWith("_statement")) && (node.TypeName != "export_statement")) {
		//	return;
		//}
		// const string langfx = ".js";
		const string langfx = "";
		SymbolMatch sym_node = sym_papa;
		if (node.TypeName == "class_declaration" ||
			node.TypeName == "function_declaration" ||
			node.TypeName == "generator_function_declaration"
			) {
			string suffix = node.TypeName.Replace("_declaration", "");
			if (node.TypeName == "generator_function_declaration") {
				suffix = "function";
			}
			var nameNode = node.Childs.FirstOrDefault(c => c.FieldName == "name");
			// 子ではなく孫にnameがある場合もあり 
			if (nameNode == null) {
				nameNode = node.Childs
					.Where(c => c.Childs != null)
					.SelectMany(c => c.Childs)
					.FirstOrDefault(g => g.FieldName == "name");
			}
			if (nameNode != null) {
				sym_node = new SymbolMatch {
					Name = nameNode.Content,
					//CaptureType = $"definition.{suffix}",
					CaptureType = $"definition{langfx}.{suffix}",
					Language = langName,
					IsPublic = true,
					IsStatic = node.Childs.Any(c => c.Content == "static"),
					Declaration = GetDeclaration(node),
					Node = node, // declarationノード自体を設定
					Papa = sym_papa
				};
				sym_papa.Childs.Add(sym_node);
				results.Add(sym_node);
			}

		} else if (node.TypeName == "field_definition") {
			string suffix = node.TypeName.Replace("_definition", "");
			var nameNode = node.Childs.FirstOrDefault(c => c.FieldName == "property");

			if (nameNode != null && sym_papa?.CaptureType == $"definition{langfx}.class") {
				sym_node = new SymbolMatch {
					Name = nameNode.Content,
					//CaptureType = $"definition.{suffix}",
					CaptureType = $"definition{langfx}.{suffix}",
					Language = langName,
					IsPublic = true,
					IsStatic = node.Childs.Any(c => c.Content == "static"),
					Declaration = GetDeclaration(node),
					Node = node, // declarationノード自体を設定
					Papa = sym_papa
				};
				sym_papa.Childs.Add(sym_node);
				results.Add(sym_node);
			}

		} else if (node.TypeName == "method_definition") {
			string suffix = node.TypeName.Replace("_definition", "");
			var nameNode = node.Childs.FirstOrDefault(c => c.FieldName == "name");

			if (nameNode != null && sym_papa?.CaptureType == $"definition{langfx}.class") {
				sym_node = new SymbolMatch {
					Name = nameNode.Content,
					//CaptureType = $"definition.{suffix}",
					CaptureType = $"definition{langfx}.{suffix}",
					Language = langName,
					IsPublic = true,
					IsStatic = node.Childs.Any(c => c.Content == "static"),
					Declaration = GetDeclaration(node),
					Node = node, // declarationノード自体を設定
					Papa = sym_papa
				};
				sym_papa.Childs.Add(sym_node);
				results.Add(sym_node);
			}
		} else if (node.TypeName == "call_expression") {
			var funcNode = node.Childs.FirstOrDefault(c => c.FieldName == "function");
			var objNode = funcNode?.Childs.FirstOrDefault(c => c.FieldName == "object");
			var propNode = funcNode?.Childs?.FirstOrDefault(c => c.FieldName == "property");
			var arguNode = node.Childs?.FirstOrDefault(c => c.FieldName == "arguments");
			var args = arguNode?.Childs?.Where(x => x.IsNamed).ToList();
			var firstArg = args?.ElementAtOrDefault(0);
			var secondArg = args?.ElementAtOrDefault(1);
			//var firstArg = arguNode?.Childs?.FirstOrDefault(x => x.IsNamed);

			if (propNode?.Content == "addEventListener" && firstArg?.TypeName == "string" && objNode != null) {
				string evntName = firstArg.Content;

				string def = $"{funcNode.Content}{GetArg(arguNode)}";

				string name = firstArg.Content + $"({objNode.Content})";
				sym_node = new SymbolMatch {
					Name = name,
					//CaptureType = $"definition.{suffix}",
					CaptureType = $"definition{langfx}.eventlistener",
					Language = langName,
					IsPublic = false,
					IsStatic = false,
					// Declaration = name,
					Declaration = def,
					Node = node, // declarationノード自体を設定
					Papa = sym_papa
				};
				sym_papa.Childs.Add(sym_node);
				results.Add(sym_node);
			} else {
				bool callop = node.Childs.Any(c => c.TypeName == "optional_chain");

				// definition{langfx}.variable 
				SymbolMatch call_papa = sym_papa;
				while (call_papa != null) {
					if (call_papa.CaptureType != $"definition{langfx}.variable")
						break;
					call_papa = call_papa.Papa;
				}
				if (call_papa != null) {
					if (propNode != null && objNode != null && firstArg?.TypeName == "string") {
						// id/class等の参照登録を行う 
						RegisterSymbolReference(langName, call_papa, node, objNode.Content, propNode.Content, firstArg.Content,
							(secondArg?.TypeName == "string") ? secondArg.Content : "");
					}

					if (funcNode?.TypeName == "identifier") {
						// 関数名 
						if (NeedRegist(funcNode.Content, "")) {
							call_papa.CallInfos.Add(new CallInfo {
								Name = funcNode.Content,
								Symbol = funcNode.Content,
								Language = langName,
								Type = "call.function",
								Node = node,
								IsOptionalCall = callop,
								IsOptionalObjectAccess = funcNode.Childs.Any(c => c.TypeName == "optional_chain")
							});
						}
					} else if (funcNode?.TypeName == "member_expression" && objNode != null && propNode?.TypeName == "property_identifier") {
						// メソッド呼び出し。
						if (NeedRegist(propNode.Content, objNode.Content)) {

							call_papa.CallInfos.Add(new CallInfo {
								Name = propNode.Content,
								Symbol = propNode.Content,
								Language = langName,
								ObjName = objNode.Content,
								Type = "call.method",
								Node = node,
								IsOptionalCall = callop,
								IsOptionalObjectAccess = funcNode.Childs.Any(c => c.TypeName == "optional_chain")
							});
						} else {

						}
					} else if (funcNode?.TypeName == "parenthesized_expression" || funcNode?.TypeName == "function_expression") {
						var paramNode = funcNode.Childs
								.Where(c => c.Childs != null)
								.SelectMany(c => c.Childs)
								.FirstOrDefault(g => g.FieldName == "parameters");
						if (paramNode == null) {
							paramNode = funcNode.FirstField("parameters");
						}
						if (paramNode != null) {
							call_papa.CallInfos.Add(new CallInfo {
								Name = "(iife)",
								Symbol = "",
								Language = langName,
								Type = "call.function",
								Node = node
							});

							sym_node = new SymbolMatch {
								Name = "(iife)",
								//CaptureType = $"definition.{suffix}",
								CaptureType = $"definition{langfx}.function",
								Language = langName,
								IsPublic = true,
								IsStatic = false,
								Declaration = "(iife)" + paramNode.Content,
								Node = node, // declarationノード自体を設定
								Papa = sym_papa
							};
							sym_papa.Childs.Add(sym_node);

						}
					}
				}
			}
		} else if (node.TypeName == "lexical_declaration" ||
				node.TypeName == "variable_declaration") {

			var dec = node.First("variable_declarator");
			var nameNode = dec?.FirstField("name");
			var valueNode = dec?.FirstField("value");

			if (nameNode != null) {
				if (valueNode?.TypeName == "function_expression" || valueNode?.TypeName == "arrow_function") {
					// 関数として扱う 
					var paramNode = valueNode.FirstField("parameters");

					var isAsync = valueNode.Childs.Any(c => c.TypeName == "async");

					string declaration = "";
					declaration += node.Childs[0].Content + " ";  // const等 
					declaration += nameNode.Content + " = ";          // 名前 
					if (isAsync) {
						declaration += "async ";
					}
					if (valueNode.TypeName == "function_expression") {
						declaration += "function ";
					}
					if (paramNode != null) {
						declaration += paramNode.Content;
					} else {
						declaration += "(...)";
					}

					sym_node = new SymbolMatch {
						Name = nameNode.Content,
						//CaptureType = $"definition.{suffix}",
						CaptureType = $"definition{langfx}.function",
						Language = langName,
						IsPublic = true,
						IsStatic = false,
						Declaration = declaration,
						Node = node, // declarationノード自体を設定
						Papa = sym_papa
					};
					sym_papa.Childs.Add(sym_node);
					results.Add(sym_node);


				} else if (valueNode?.TypeName == "class" || valueNode?.TypeName == "object") {
					// クラスとして扱う 
					string declaration = "";
					declaration += node.Childs[0].Content + " ";  // const等 
					declaration += nameNode.Content + " = ";          // 名前 
					if (valueNode.TypeName == "class") {
						declaration += "class {...}";
					} else {
						declaration += "{...}";
					}

					bool isClass = false;
					if (valueNode.TypeName == "class") {
						isClass = true;
					} else if (valueNode.TypeName == "object") {
						if (valueNode.First("method_definition") != null) {
							isClass = true;
						} else {
							var pairAll = valueNode.All("pair");
							bool hasMethod = pairAll.Any(pair =>
							pair.Childs.Any(c =>
								c.TypeName == "function_expression" ||
								c.TypeName == "arrow_function"));
							if (hasMethod) {
								isClass = true;
							}
						}
					}
					if (isClass) {
						sym_node = new SymbolMatch {
							Name = nameNode.Content,
							//CaptureType = $"definition.{suffix}",
							CaptureType = $"definition{langfx}.class",
							Language = langName,
							IsPublic = true,
							IsStatic = false,
							Declaration = declaration,
							Node = node, // declarationノード自体を設定
							Papa = sym_papa
						};
						sym_papa.Childs.Add(sym_node);
						results.Add(sym_node);
					} else {
						// 変数関係 
						sym_node = new SymbolMatch {
							Name = nameNode.Content,
							//CaptureType = $"definition.{suffix}",
							CaptureType = $"definition{langfx}.variable",
							Language = langName,
							IsPublic = true,
							IsStatic = false,
							Declaration = GetDeclaration(node),
							Node = node,
							Papa = sym_papa
						};
						// sym_papa.Childs.Add(sym_node);
						results.Add(sym_node);
					}
				} else {
					// 変数関係 
					sym_node = new SymbolMatch {
						Name = nameNode.Content,
						//CaptureType = $"definition.{suffix}",
						CaptureType = $"definition{langfx}.variable",
						Language = langName,
						IsPublic = true,
						IsStatic = false,
						Declaration = GetDeclaration(node),
						Node = node,
						Papa = sym_papa
					};
					// sym_papa.Childs.Add(sym_node);
					results.Add(sym_node);
				}
			}


		} else if (node.TypeName == "pair") {
			if (node.Papa?.TypeName == "object" && sym_papa?.CaptureType == $"definition{langfx}.class") {
				var keyNode = node.FirstField("key");
				var valueNode = node.FirstField("value");
				if (keyNode != null && (valueNode?.TypeName == "function_expression" || valueNode?.TypeName == "arrow_function")) {

					var paramNode = valueNode.FirstField("parameters");
					var isAsync = valueNode.Childs.Any(c => c.TypeName == "async");

					string declaration = "";

					declaration += keyNode.Content + ": ";
					if (isAsync) {
						declaration += "async ";
					}
					if (valueNode?.TypeName == "function_expression") {
						declaration += "function ";
					}
					if (paramNode != null) {
						declaration += paramNode.Content;
					} else {
						declaration += "(...)";
					}
					sym_node = new SymbolMatch {
						Name = keyNode.Content,
						//CaptureType = $"definition.{suffix}",
						CaptureType = $"definition{langfx}.method",
						Language = langName,
						IsPublic = true,
						IsStatic = false,
						Declaration = declaration,
						Node = node, // declarationノード自体を設定
						Papa = sym_papa
					};
					sym_papa.Childs.Add(sym_node);
					results.Add(sym_node);
				}
			}
		} else if (node.TypeName == "assignment_expression") {
			var leftNode = node.FirstField("left");
			var propNode = leftNode?.FirstField("property");
			var objNode = leftNode?.FirstField("object");
			var rightNode = node.FirstField("right");

			if ((objNode?.Content == "window" || objNode?.Content == "document") &&
				(rightNode?.TypeName == "arrow_function" || rightNode?.TypeName == "function_expression" || rightNode?.TypeName == "object" || rightNode?.TypeName == "class") &&
				propNode != null) {

				string ctype = "";
				string name = "";
				string declaration = "";

				if (rightNode?.TypeName == "object" || rightNode?.TypeName == "class") {
					if (rightNode?.TypeName == "class") {
						ctype = $"definition{langfx}.class";
						name = propNode.Content;
						declaration = leftNode.Content + " = {...}";
					} else {
						bool isClass = false;
						if (rightNode.First("method_definition") != null) {
							isClass = true;
						} else {
							var pairAll = rightNode.All("pair");
							bool hasMethod = pairAll.Any(pair =>
							pair.Childs.Any(c =>
								c.TypeName == "function_expression" ||
								c.TypeName == "arrow_function"));
							if (hasMethod) {
								isClass = true;
							}
						}
						if (isClass) {
							ctype = $"definition{langfx}.class";
							name = propNode.Content;
							declaration = leftNode.Content + " = {...}";
						}
					}


				} else if (propNode.Content.StartsWith("on")) {
					ctype = $"definition{langfx}.eventlistener";
					name = propNode.Content + "(" + objNode?.Content + ")";
					declaration = leftNode.Content + " = " + GetDeclaration(rightNode);
				} else {
					ctype = $"definition{langfx}.function";
					name = propNode.Content;
					declaration = GetDeclaration(node);
				}

				if (!string.IsNullOrEmpty(ctype)) {

					sym_node = new SymbolMatch {
						Name = name,
						//CaptureType = $"definition.{suffix}",
						CaptureType = ctype,
						Language = langName,
						IsPublic = true,
						IsStatic = false,
						Declaration = declaration,
						Node = node, // declarationノード自体を設定
						Papa = sym_papa
					};
					sym_papa.Childs.Add(sym_node);
					results.Add(sym_node);
				}

			} else {
				// el.className = 'copy-flash';のようなのを想定 
				SymbolMatch call_papa = sym_papa;
				while (call_papa != null) {
					if (call_papa.CaptureType != $"definition{langfx}.variable")
						break;
					call_papa = call_papa.Papa;
				}
				if (call_papa != null && leftNode != null && rightNode.TypeName == "string") {
					RegisterSymbolReference(langName, call_papa, node, leftNode.Content, "=", rightNode.Content, "");
				}
			}

		} else if (node.TypeName == "identifier" && node.FieldName != "name" && node.FieldName != "function") {
			sym_papa.PendingCallInfos.Add(new CallInfo {
				Name = node.Content,
				Symbol = node.Content,
				Language = langName,
				Type = "ref.symbol",
				Visible = false,
				Node = node
			});

		}


		foreach (var child in node.Childs) {
			if (child.Content.IndexOf("!function") != -1) {
				bool need_break = true;
			}
			ExtractSymbolRecursive(langName, child, results, sym_node);
		}
	}

	static void RegisterSymbolReference(LanguageId langName, SymbolMatch call_papa, TreeSitterElement node, string objPart, string funcName, 
		string firstArgu, string secondArgu)
	{
		if ((firstArgu.StartsWith("\"") && firstArgu.EndsWith("\"")) ||
			(firstArgu.StartsWith("'") && firstArgu.EndsWith("'"))) {
			firstArgu = firstArgu.Substring(1, firstArgu.Length - 2);
		}
		if ((secondArgu.StartsWith("\"") && secondArgu.EndsWith("\"")) ||
			(secondArgu.StartsWith("'") && secondArgu.EndsWith("'"))) {
			secondArgu = secondArgu.Substring(1, secondArgu.Length - 2);
		}
		// -----------------------------
		// 1. classList 系
		// -----------------------------
		if (objPart.EndsWith(".classList")) {
			switch (funcName) {
				case "add":
				case "remove":
				case "toggle":
				case "contains":
					call_papa.CallInfos.Add(new CallInfo {
						Name = "." + firstArgu,
						Symbol = "." + firstArgu,
						Language = langName,
						Type = "ref.symbol",
						Node = node
					});
					break;
			}
			return;
		}
		// -----------------------------
		// 2. className = "xxx"
		// -----------------------------
		if (objPart.EndsWith(".className") && funcName == "=") {
			call_papa.CallInfos.Add(new CallInfo {
				Name = "." + firstArgu,
				Symbol = "." + firstArgu,
				Language = langName,
				Type = "ref.symbol",
				Node = node
			});
			return;
		}

		// -----------------------------
		// 3. setAttribute("class", "xxx")
		// -----------------------------
		if (objPart.EndsWith(".setAttribute") && funcName == "setAttribute") {
			if (firstArgu == "class") {
				string value = secondArgu;
				if (!string.IsNullOrEmpty(value)) {
					call_papa.CallInfos.Add(new CallInfo {
						Name = "." + value,
						Symbol = "." + value,
						Language = langName,
						Type = "ref.symbol",
						Node = node
					});
				}
			}
			return;
		}
		// -----------------------------
		// 4. id 系 getElementById("xxx")
		// -----------------------------
		if (funcName == "getElementById") {
			call_papa.CallInfos.Add(new CallInfo {
				Name = "#" + firstArgu,
				Symbol = "#" + firstArgu,
				Language = langName,
				Type = "ref.symbol",
				Node = node
			});
			return;
		}

		// el.id = "xxx"
		if (objPart.EndsWith(".id") && funcName == "=") {
			call_papa.CallInfos.Add(new CallInfo {
				Name = "#" + firstArgu,
				Symbol = "#" + firstArgu,
				Language = langName,
				Type = "ref.symbol",
				Node = node
			});
			return;
		}

		// -----------------------------
		// 5. CSS セレクタ系
		// -----------------------------
		// querySelector(".xxx") / "#xxx"
		if (funcName == "querySelector" ||
			funcName == "querySelectorAll" ||
			funcName == "matches" ||
			funcName == "closest") {
			// argu が ".xxx" or "#xxx" のまま使える
			call_papa.CallInfos.Add(new CallInfo {
				Name = firstArgu,
				Symbol = firstArgu,
				Language = langName,
				Type = "ref.symbol",
				Node = node
			});
			return;
		}
	}


	static string GetArg(TreeSitterElement node)
	{
		StringBuilder sb = new StringBuilder();
		foreach (var child in node.Childs) {
			if (child.TypeName == "arrow_function" || child.TypeName == "function_expression") {
				foreach (var grandchild in child.Childs) {
					if (grandchild.TypeName == "statement_block") {
						break;
					}
					sb.Append($"{grandchild.Content}");
				}
				sb.Append("{...})");
				break;
			} 
			if (child.TypeName == "object") {
				sb.Append("{...})");
				break;
			}

			sb.Append($"{child.Content}");
		}
		return sb.ToString();
	}

	static string GetDeclaration(TreeSitterElement node)
	{
		StringBuilder sb = new StringBuilder();

		foreach (var child in node.Childs) {
			if (child.TypeName == "statement_block" || child.TypeName == "accessor_list" || child.TypeName == "block" || child.TypeName == "class_body") {
				break;
			}
			sb.Append($" {child.Content}");
		}
		return sb.ToString();
	}
	static bool NeedRegist(string name, string objname)
	{
		// オブジェクト名だけ取り出す
		string obj = objname;
		int pos = obj.IndexOf('.');
		if (pos >= 0)
			obj = obj.Substring(0, pos);

		if (objname.EndsWith(".classList")) {
			return false;
		}

		// DOM API
		//if (obj == "document") {
			switch (name) {
				case "getElementById":
				case "getElementsByClassName":
				case "getElementsByTagName":
				case "querySelector":
				case "querySelectorAll":
				case "createElement":
				case "createTextNode":
				case "addEventListener":
				case "removeEventListener":
					return false;
			}
		//}

		// Element API
		switch (name) {
			case "addEventListener":
			case "removeEventListener":
			case "appendChild":
			case "removeChild":
			case "insertBefore":
			case "replaceChild":
			case "setAttribute":
			case "getAttribute":
			case "removeAttribute":
			case "querySelector":
			case "querySelectorAll":
			case "focus":
			case "blur":
			case "click":
				return false;
		}

		// console
		if (obj == "console")
			return false;

		// JSON
		if (obj == "JSON")
			return false;

		// Math
		if (obj == "Math")
			return false;

		// Array
		switch (name) {
			case "map":
			case "filter":
			case "reduce":
			case "forEach":
			case "find":
			case "findIndex":
			case "some":
			case "every":
			case "sort":
			case "push":
			case "pop":
			case "shift":
			case "unshift":
			case "splice":
			case "slice":
			case "concat":
			case "isArray":
				return false;
		}

		// String
		switch (name) {
			case "substring":
			case "substr":
			case "slice":
			case "split":
			case "trim":
			case "replace":
			case "replaceAll":
			case "toLowerCase":
			case "toUpperCase":
			case "startsWith":
			case "endsWith":
			case "includes":
			case "indexOf":
			case "join":
				return false;
		}

		// Promise
		switch (name) {
			case "then":
			case "catch":
			case "finally":
				return false;
		}

		// Timer
		switch (name) {
			case "setTimeout":
			case "setInterval":
			case "clearTimeout":
			case "clearInterval":
				return false;
		}
		switch (name) {
			case "parseInt":
			case "parseFloat":
			case "Number":
			case "isNaN":
			case "isFinite":
			case "encodeURI":
			case "decodeURI":
			case "encodeURIComponent":
			case "decodeURIComponent":
			case "toFixed":
			case "json":
			case "alert":
			case "String":
			case "Boolean":
			case "Object":
			case "Array":

				// case "eval":
				return false;
		}

		switch (name) {
			case "toLocaleString":

				return false;
		}

		return true;
	}


}




public class CallGraphBuilderGemini
{
	// コールグラフの表現: Dictionary<呼び出し元関数名, 呼び出し先関数名のリスト>
	public Dictionary<string, HashSet<string>> CallGraph { get; private set; } = new Dictionary<string, HashSet<string>>();

	public void Build(TreeSitterElement root)
	{
		// 最初の関数コンテキストはグローバル（トップレベル）
		Traverse(root, "global");
	}

	private void Traverse(TreeSitterElement element, string currentFunction)
	{
		if (element == null)
			return;

		string nextFunctionContext = currentFunction;

		// 1. 新しい関数の定義を見つけた場合、コンテキストを更新する
		if (IsFunctionDefinition(element)) {
			string funcName = ExtractFunctionName(element);
			if (!string.IsNullOrEmpty(funcName)) {
				nextFunctionContext = funcName;
				// グラフにノードを初期登録
				if (!CallGraph.ContainsKey(nextFunctionContext)) {
					CallGraph[nextFunctionContext] = new HashSet<string>();
				}
			}
		}
		// 2. 関数呼び出しを見つけた場合、現在のコンテキストから呼び出し先へエッジを張る
		else if (element.TypeName == "call_expression") {
			string calleeName = ExtractCalleeName(element);
			if (!string.IsNullOrEmpty(calleeName)) {
				// 呼び出し元がまだ登録されていなければ追加
				if (!CallGraph.ContainsKey(currentFunction)) {
					CallGraph[currentFunction] = new HashSet<string>();
				}
				CallGraph[currentFunction].Add(calleeName);
			}
		}

		// 子ノードを再帰的に探索（更新されたコンテキストを引き継ぐ）
		foreach (var child in element.Childs) {
			Traverse(child, nextFunctionContext);
		}
	}

	// JavaScriptにおける関数定義ノードの判定
	private bool IsFunctionDefinition(TreeSitterElement element)
	{
		return element.TypeName == "function_declaration" ||
			   element.TypeName == "function_expression" ||
			   element.TypeName == "arrow_function";
	}

	// 関数定義ノードから関数名を抽出する
	private string ExtractFunctionName(TreeSitterElement element)
	{
		// function myFunc() {} のケース
		if (element.TypeName == "function_declaration") {
			var nameNode = element.Childs.Find(c => c.TypeName == "identifier");
			if (nameNode != null)
				return nameNode.Content;
		}

		// const myFunc = () => {} や const x = function() {} のケース
		// 親（Papa）が variable_declarator であるか、さらにその上が lexical_declaration かをチェック
		if (element.Papa != null && element.Papa.TypeName == "variable_declarator") {
			var nameNode = element.Papa.Childs.Find(c => c.TypeName == "identifier");
			if (nameNode != null)
				return nameNode.Content;
		}

		return "anonymous_" + element.StartByte; // 匿名関数の場合
	}

	// call_expression ノードから呼び出されている関数名を抽出する
	private string ExtractCalleeName(TreeSitterElement element)
	{
		// call_expression の最初の子ノード（通常は identifier や member_expression）が呼び出し先
		if (element.Childs.Count > 0) {
			var functionNode = element.Childs[0];

			// 単純な関数呼び出し: func()
			if (functionNode.TypeName == "identifier") {
				return functionNode.Content;
			}
			// メソッド呼び出し: console.log() や obj.method()
			else if (functionNode.TypeName == "member_expression") {
				// 簡易的に content をそのまま返す（"console.log" などになる）
				return functionNode.Content;
			}
		}
		return null;
	}

	// 結果を出力するデバッグ用メソッド
	public void PrintGraph()
	{
		foreach (var kvp in CallGraph) {
			Console.WriteLine($"Function [{kvp.Key}] calls:");
			foreach (var callee in kvp.Value) {
				Console.WriteLine($"  -> {callee}");
			}
		}
	}
}