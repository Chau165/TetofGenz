# Starts MCP servers for Tet Holiday Adventure (Unity + Blender).
# Run from project root: powershell -ExecutionPolicy Bypass -File .\start-mcp.ps1

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$Uvx = "$env:USERPROFILE\.local\bin\uvx.exe"
$Blender = "C:\Program Files\Blender Foundation\Blender 5.1\blender.exe"

function Test-Port([int]$Port) {
    return (Test-NetConnection localhost -Port $Port -WarningAction SilentlyContinue).TcpTestSucceeded
}

Write-Host "=== Unity MCP (HTTP 8090) ==="
if (-not (Test-Port 8090)) {
    if (-not (Test-Path $Uvx)) { throw "uvx not found at $Uvx" }
    Start-Process -FilePath $Uvx -ArgumentList @(
        "--from", "mcpforunityserver==9.7.1",
        "mcp-for-unity",
        "--transport", "http",
        "--http-url", "http://127.0.0.1:8090",
        "--project-scoped-tools"
    ) -WorkingDirectory $ProjectRoot -WindowStyle Minimized
    Write-Host "Started mcp-for-unity on :8090"
} else {
    Write-Host "Port 8090 already listening"
}

Write-Host "=== Blender MCP (socket 9876) ==="
if (-not (Test-Port 9876)) {
    if (-not (Test-Path $Blender)) { throw "Blender not found at $Blender" }
    # GUI instance (not --background): addon needs Blender's main loop for MCP timers.
    Start-Process -FilePath $Blender -ArgumentList @(
        "--python", "$ProjectRoot\blender_mcp_boot.py"
    ) -WindowStyle Minimized
    Write-Host "Started Blender (minimized) with MCP addon on :9876"
} else {
    Write-Host "Port 9876 already listening"
}

Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. Open Unity project (Window > AI Game Developer — MCP to confirm server on :28711)"
Write-Host "  2. In Cursor: Settings > MCP > Restart servers (or Ctrl+Shift+P > Developer: Reload Window)"
Write-Host "  3. Unity MCP URL: http://127.0.0.1:8090/mcp"
Write-Host "  4. AI Game Developer URL: http://localhost:28711"
