# PowerShell script to convert Assert.True(false, "SKIPPED:...") patterns to [Fact(Skip = "...")]

$files = @{
    "tests\MixingStation.Client.IntegrationTests\Console\ConsoleClientPhase6IntegrationTests.cs" = @(
        @{
            TestName = "PostMeteringSubscribeAsync_RequiresMixer"
            Skip = "Metering subscription requires physical mixer connection"
        },
        @{
            TestName = "PostMeteringUnsubscribeAsync_RequiresMixer"
            Skip = "Metering operations require physical mixer connection"
        },
        @{
            TestName = "PostMetering2SubscribeAsync_RequiresMixer"
            Skip = "Metering2 subscription requires physical mixer connection"
        }
    );
    "tests\MixingStation.Client.IntegrationTests\App\AppClientPhase1IntegrationTests.cs" = @(
        @{
            TestName = "PostMixersConnectAsync_WithValidData_Succeeds"
            Skip = "Requires physical mixer hardware and IP address"
        }
    );
    "tests\MixingStation.Client.IntegrationTests\App\AppClientPhase7IntegrationTests.cs" = @(
        @{
            TestName = "PostPresetsChannelApplyAsync_RequiresMixer"
            Skip = "Requires physical mixer with valid preset data"
        },
        @{
            TestName = "PostPresetsChannelCreateAsync_RequiresMixer"
            Skip = "Requires physical mixer with valid channel"
        },
        @{
            TestName = "PostPresetsScenesApplyAsync_RequiresMixer"
            Skip = "Requires physical mixer with valid scene data"
        },
        @{
            TestName = "PostPresetsScenesCreateAsync_RequiresMixer"
            Skip = "Requires physical mixer connection"
        }
    );
    "tests\MixingStation.Client.IntegrationTests\App\AppClientPhase8IntegrationTests.cs" = @(
        @{
            TestName = "PostIdcasAsync_Create_RequiresMixer"
            Skip = "IDCA operations require physical mixer connection"
        },
        @{
            TestName = "PostIdcasAsync_Modify_RequiresMixer"
            Skip = "IDCA operations require physical mixer connection"
        },
        @{
            TestName = "PostIdcasDeleteAsync_RequiresMixer"
            Skip = "IDCA operations require physical mixer connection"
        },
        @{
            TestName = "PostIdcasRearrangeAsync_RequiresMixer"
            Skip = "IDCA operations require physical mixer connection"
        },
        @{
            TestName = "GetUiSelectedChannelAsync_WithParam_RequiresMixer"
            Skip = "Setting selected channel requires physical mixer connection"
        }
    );
    "tests\MixingStation.Client.IntegrationTests\Console\ConsoleClientPhase2IntegrationTests.cs" = @(
        @{
            TestName = "PostDataSubscribeAsync_RequiresMixer"
            Skip = "Data subscription requires physical mixer connection"
        },
        @{
            TestName = "PostDataSetAsync_RequiresMixer"
            Skip = "Setting data requires physical mixer connection"
        }
    );
    "tests\MixingStation.Client.IntegrationTests\Console\ConsoleClientPhase4IntegrationTests.cs" = @(
        @{
            TestName = "GetDataDefinitionsAsync_RequiresMixer"
            Skip = "Data definitions require data paths from physical mixer"
        },
        @{
            TestName = "GetDataDefinitions2Async_RequiresMixer"
            Skip = "Data definitions require data paths from physical mixer"
        },
        @{
            TestName = "PostDataUnsubscribeAsync_RequiresMixer"
            Skip = "Data subscription requires WebSocket connection (not available in REST mode)"
        }
    );
    "tests\MixingStation.Client.IntegrationTests\Console\ConsoleClientPhase5IntegrationTests.cs" = @(
        @{
            TestName = "PostAuthLoginAsync_RequiresMixer"
            Skip = "Login requires physical mixer and valid credentials"
        }
    );
    "tests\MixingStation.Client.IntegrationTests\Console\ConsoleClientPhase10IntegrationTests.cs" = @(
        @{
            TestName = "GetOnConfigChangedAsync_WebSocketEndpoint"
            Skip = "WebSocket endpoint not available in REST mode"
        }
    )
}

foreach ($file in $files.Keys) {
    $fullPath = Join-Path "d:\Repos\MixingStation-API-Client" $file
    Write-Host "Processing: $file"
    
    if (Test-Path $fullPath) {
        $content = Get-Content $fullPath -Raw
        $modified = $false
        
        foreach ($test in $files[$file]) {
            # Find the [Fact] attribute before the test method and replace with [Fact(Skip = "...")]
            $pattern = "(\s+)\[Fact\]\s+public async Task $($test.TestName)\("
            $replacement = "`$1[Fact(Skip = `"$($test.Skip)`")]`r`n`$1public async Task $($test.TestName)("
            
            if ($content -match $pattern) {
                $content = $content -replace $pattern, $replacement
                $modified = $true
                Write-Host "  - Converted: $($test.TestName)"
            }
        }
        
        if ($modified) {
            # Also remove the Assert.True(false, "SKIPPED:...") lines and await RequireServerAsync()
            $content = $content -replace '\s+// Arrange\s+await RequireServerAsync\(\);\s+', "`r`n        "
            $content = $content -replace '\s+await RequireServerAsync\(\);\s+', "`r`n        "
            $content = $content -replace '\s+//[^\r\n]*NOTE:[^\r\n]*\s+', "`r`n        "
            $content = $content -replace '\s+//[^\r\n]*Skipping[^\r\n]*\s+', "`r`n        "
            $content = $content -replace '\s+Assert\.True\(false, "SKIPPED:[^"]*"\);\s+', "        await Task.CompletedTask;`r`n        "
            
            Set-Content -Path $fullPath -Value $content -NoNewline
            Write-Host "  UPDATED"
        }
    }
}

Write-Host "`nDone!"
