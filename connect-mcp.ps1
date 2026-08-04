# One-shot: start MCP servers, configure Cursor, verify connection.
# Run from project root: .\connect-mcp.ps1

$ErrorActionPreference = "Stop"
$ProjectRoot = $PSScriptRoot

Write-Host "=== Connect Unity MCP to Cursor ===" -ForegroundColor Cyan

& "$ProjectRoot\start-mcp.ps1"

Write-Host ""
Write-Host "=== AI Game Developer MCP (port 28711) ===" -ForegroundColor Cyan
Write-Host "Configuring Cursor MCP for Ivan Murzak plugin..."
npx unity-mcp-cli setup-mcp cursor . 2>&1 | ForEach-Object { Write-Host $_ }

Write-Host ""
Write-Host "=== Cursor MCP config ===" -ForegroundColor Cyan
Write-Host "Using project .cursor/mcp.json"
Write-Host "  unityMCP          -> http://127.0.0.1:8090/mcp"
Write-Host "  ai-game-developer -> http://localhost:28711 (Unity must be open)"

Write-Host ""
& "$ProjectRoot\verify-mcp.ps1"
