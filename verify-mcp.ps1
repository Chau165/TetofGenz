# Unity MCP connectivity check for TetHolidayAdventure.
# Run: .\verify-mcp.ps1

$ErrorActionPreference = "Continue"

function Test-McpHttp([string]$Name, [string]$Url, [hashtable]$ExtraHeaders) {
    $headers = @{
        "Accept"       = "application/json, text/event-stream"
        "Content-Type" = "application/json"
    }
    if ($ExtraHeaders) {
        foreach ($key in $ExtraHeaders.Keys) { $headers[$key] = $ExtraHeaders[$key] }
    }

    $body = @{
        jsonrpc = "2.0"
        id      = 1
        method  = "initialize"
        params  = @{
            protocolVersion = "2024-11-05"
            capabilities    = @{}
            clientInfo      = @{ name = "verify-mcp"; version = "1.0" }
        }
    } | ConvertTo-Json -Depth 6 -Compress

    try {
        $response = Invoke-WebRequest -Uri $Url -Method POST -Headers $headers -Body $body -UseBasicParsing -TimeoutSec 5
        if ($response.StatusCode -eq 200) {
            Write-Host "OK: $Name responded (HTTP $($response.StatusCode))" -ForegroundColor Green
            return $true
        }
        Write-Host "WARN: $Name returned HTTP $($response.StatusCode)" -ForegroundColor Yellow
        return $false
    }
    catch {
        Write-Host "FAIL: $Name — $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

Write-Host "=== Unity MCP verify ===" -ForegroundColor Cyan

$coplayOk = $false
$ivanOk = $false

if ((Test-NetConnection localhost -Port 8090 -WarningAction SilentlyContinue).TcpTestSucceeded) {
    Write-Host "OK: Port 8090 listening (CoplayDev MCP for Unity)"
    $coplayOk = Test-McpHttp "unityMCP (CoplayDev)" "http://127.0.0.1:8090/mcp" $null
}
else {
    Write-Host "FAIL: Port 8090 not listening." -ForegroundColor Red
    Write-Host "  Fix: Open Unity > Window > MCP for Unity > Start Server"
    Write-Host "  Or:  .\start-mcp.ps1"
}

if ((Test-NetConnection localhost -Port 28711 -WarningAction SilentlyContinue).TcpTestSucceeded) {
    Write-Host "OK: Port 28711 listening (AI Game Developer / Ivan Murzak)"
    $token = $null
    $mcpPath = Join-Path $PSScriptRoot ".cursor\mcp.json"
    if (Test-Path $mcpPath) {
        $cfg = Get-Content $mcpPath -Raw | ConvertFrom-Json
        $token = $cfg.mcpServers.'ai-game-developer'.headers.Authorization
    }
    $hdr = @{}
    if ($token) { $hdr["Authorization"] = $token }
    $ivanOk = Test-McpHttp "ai-game-developer" "http://127.0.0.1:28711" $hdr
}
else {
    Write-Host "SKIP: Port 28711 not listening (optional Ivan Murzak plugin)." -ForegroundColor DarkYellow
    Write-Host "  After Unity imports com.ivanmurzak.unity.mcp, run:"
    Write-Host "    npx unity-mcp-cli setup-mcp cursor ."
}

Write-Host ""
if ($coplayOk -or $ivanOk) {
    Write-Host "MCP ready for Cursor." -ForegroundColor Green
    Write-Host "  1. Cursor Settings > MCP > enable unityMCP"
    Write-Host "  2. Ctrl+Shift+P > Developer: Reload Window"
    Write-Host "  3. Unity: Window > MCP for Unity > Session Active"
    exit 0
}

Write-Host "No MCP server reachable. Start Unity + MCP server first." -ForegroundColor Red
exit 1
