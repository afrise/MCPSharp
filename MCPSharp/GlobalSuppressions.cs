// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0130:Namespace does not match folder structure", Justification = "<Pending>", Scope = "namespace", Target = "~N:MCPSharp")]
[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>", Scope = "member", Target = "~M:MCPSharp.Core.ToolHandler`1.HandleAsync(System.Collections.Generic.Dictionary{System.String,System.Object},System.Threading.CancellationToken)~System.Threading.Tasks.Task{MCPSharp.Model.Results.CallToolResult}")]

// the equivalent of #pragma warning disable CS1591
[assembly: SuppressMessage("Style", "CS1591:Missing XML comment for publicly visible type or member", Justification = "<Pending>", Scope = "namespace", Target = "~N:MCPSharp")]
[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>", Scope = "member", Target = "~M:MCPSharp.MCPServer.Initialize(System.String,MCPSharp.Model.Capabilities.ClientCapabilities,MCPSharp.Model.Implementation)~MCPSharp.Model.Results.InitializeResult")]
