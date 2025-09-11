param(
    [Parameter(Mandatory=$true)]
    [string]$ProjectPath,
    [switch]$GenerateReport
)

Write-Host "🔍 Clean Architecture Audit Starting..." -ForegroundColor Cyan
Write-Host "Project Path: $ProjectPath" -ForegroundColor Gray

# Initialize results
$results = @{
    Score = 0
    MaxScore = 100
    Issues = @()
    Successes = @()
}

function Test-DomainDependencies {
    param([string]$domainPath)
    
    Write-Host "`n📁 Checking Domain Layer Dependencies..." -ForegroundColor Yellow
    
    $domainProjects = Get-ChildItem -Path $domainPath -Filter "*.csproj" -Recurse -ErrorAction SilentlyContinue
    
    foreach ($project in $domainProjects) {
        $content = Get-Content $project.FullName -Raw
        
        if ($content -match '<PackageReference|<ProjectReference.*Infrastructure|<ProjectReference.*Application') {
            $results.Issues += "❌ Domain project '$($project.Name)' has external dependencies"
            Write-Host "  ❌ $($project.Name): Has external dependencies" -ForegroundColor Red
        } else {
            $results.Successes += "✅ Domain project '$($project.Name)' is dependency-free"
            $results.Score += 20
            Write-Host "  ✅ $($project.Name): Clean dependencies" -ForegroundColor Green
        }
    }
}

function Test-InterfaceLocation {
    param([string]$projectPath)
    
    Write-Host "`n🎯 Checking Interface Locations..." -ForegroundColor Yellow
    
    # Look for repository interfaces in Infrastructure (bad)
    $infraInterfaces = Get-ChildItem -Path "$projectPath\*Infrastructure*" -Filter "*Repository*.cs" -Recurse -ErrorAction SilentlyContinue
    
    foreach ($file in $infraInterfaces) {
        $content = Get-Content $file.FullName -Raw
        if ($content -match 'interface.*Repository') {
            $results.Issues += "❌ Repository interface found in Infrastructure: $($file.Name)"
            Write-Host "  ❌ $($file.Name): Interface in wrong location (Infrastructure)" -ForegroundColor Red
        }
    }
    
    # Look for repository interfaces in Domain (good)
    $domainInterfaces = Get-ChildItem -Path "$projectPath\*Domain*" -Filter "*Repository*.cs" -Recurse -ErrorAction SilentlyContinue
    
    foreach ($file in $domainInterfaces) {
        $content = Get-Content $file.FullName -Raw
        if ($content -match 'interface.*Repository') {
            $results.Successes += "✅ Repository interface found in Domain: $($file.Name)"
            $results.Score += 15
            Write-Host "  ✅ $($file.Name): Interface in correct location (Domain)" -ForegroundColor Green
        }
    }
}

function Test-TestPerformance {
    param([string]$projectPath)
    
    Write-Host "`n⚡ Analyzing Test Performance..." -ForegroundColor Yellow
    
    # Look for database context usage in unit tests
    $testFiles = Get-ChildItem -Path "$projectPath\*Test*" -Filter "*.cs" -Recurse -ErrorAction SilentlyContinue
    
    foreach ($file in $testFiles) {
        $content = Get-Content $file.FullName -Raw
        
        # Check for database usage in tests
        if ($content -match 'DbContext|UseSqlServer|UseInMemoryDatabase' -and $content -notmatch 'Integration') {
            $results.Issues += "❌ Unit test using database: $($file.Name)"
            Write-Host "  ❌ $($file.Name): Unit test depends on database" -ForegroundColor Red
        }
        
        # Check for proper unit tests (domain logic only)
        if ($content -match '\[Test\].*new User\(' -and $content -notmatch 'DbContext') {
            $results.Successes += "✅ True unit test found: $($file.Name)"
            $results.Score += 10
            Write-Host "  ✅ $($file.Name): Proper unit test (no dependencies)" -ForegroundColor Green
        }
    }
}

function Test-LayerComplexity {
    param([string]$projectPath)
    
    Write-Host "`n🏗️ Analyzing Layer Complexity..." -ForegroundColor Yellow
    
    # Look for excessive mapping in controllers
    $controllerFiles = Get-ChildItem -Path "$projectPath" -Filter "*Controller.cs" -Recurse -ErrorAction SilentlyContinue
    
    foreach ($file in $controllerFiles) {
        $content = Get-Content $file.FullName -Raw
        
        # Count mapping operations
        $mappingCount = ($content | Select-String -Pattern '_mapper\.Map' -AllMatches).Matches.Count
        
        if ($mappingCount -gt 2) {
            $results.Issues += "❌ Excessive mapping in controller: $($file.Name) ($mappingCount mappings)"
            Write-Host "  ❌ $($file.Name): Too many mappings ($mappingCount)" -ForegroundColor Red
        } elseif ($mappingCount -eq 0) {
            # Check for direct projection
            if ($content -match '\.Select\(.*new.*\)') {
                $results.Successes += "✅ Direct projection used: $($file.Name)"
                $results.Score += 15
                Write-Host "  ✅ $($file.Name): Uses direct projection" -ForegroundColor Green
            }
        }
    }
}

function Test-InterfaceOveruse {
    param([string]$projectPath)
    
    Write-Host "`n🎭 Checking for Interface Overuse..." -ForegroundColor Yellow
    
    # Count interfaces and their implementations
    $interfaces = Get-ChildItem -Path $projectPath -Filter "*.cs" -Recurse -ErrorAction SilentlyContinue | Where-Object {
        (Get-Content $_.FullName -Raw) -match 'interface\s+I[A-Z]'
    }
    
    $interfaceCount = $interfaces.Count
    
    if ($interfaceCount -gt 20) {
        $results.Issues += "❌ Too many interfaces: $interfaceCount (consider consolidation)"
        Write-Host "  ❌ Interface count: $interfaceCount (consider reducing)" -ForegroundColor Red
    } else {
        $results.Successes += "✅ Reasonable interface count: $interfaceCount"
        $results.Score += 10
        Write-Host "  ✅ Interface count: $interfaceCount (reasonable)" -ForegroundColor Green
    }
}

# Run all audits
Test-DomainDependencies -domainPath "$ProjectPath\*Domain*"
Test-InterfaceLocation -projectPath $ProjectPath
Test-TestPerformance -projectPath $ProjectPath  
Test-LayerComplexity -projectPath $ProjectPath
Test-InterfaceOveruse -projectPath $ProjectPath

# Generate final report
Write-Host "`n" -NoNewline
Write-Host "📊 AUDIT RESULTS" -ForegroundColor Cyan
Write-Host "=================" -ForegroundColor Cyan

$percentage = [math]::Round(($results.Score / $results.MaxScore) * 100, 1)
$color = if ($percentage -ge 80) { "Green" } elseif ($percentage -ge 60) { "Yellow" } else { "Red" }

Write-Host "Overall Score: $($results.Score)/$($results.MaxScore) ($percentage`%)" -ForegroundColor $color

Write-Host "`n✅ SUCCESSES ($($results.Successes.Count)):" -ForegroundColor Green
$results.Successes | ForEach-Object { Write-Host "  $_" -ForegroundColor Green }

if ($results.Issues.Count -gt 0) {
    Write-Host "`n❌ ISSUES TO FIX ($($results.Issues.Count)):" -ForegroundColor Red
    $results.Issues | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
}

Write-Host "`n🎯 RECOMMENDATIONS:" -ForegroundColor Cyan
if ($percentage -lt 80) {
    Write-Host "  • Move repository interfaces from Infrastructure to Domain" -ForegroundColor White
    Write-Host "  • Remove database dependencies from unit tests" -ForegroundColor White
    Write-Host "  • Use direct projections instead of multiple mappings" -ForegroundColor White
    Write-Host "  • Consolidate unnecessary interfaces" -ForegroundColor White
} else {
    Write-Host "  🎉 Architecture is in excellent shape!" -ForegroundColor Green
    Write-Host "  • Keep following clean architecture principles" -ForegroundColor White
    Write-Host "  • Regular audits help maintain quality" -ForegroundColor White
}

if ($GenerateReport) {
    $reportPath = "$ProjectPath\architecture-audit-report.md"
    
    $reportContent = @"
# Architecture Audit Report
Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

## Overall Score: $($results.Score)/$($results.MaxScore) ($percentage%)

## Successes ✅
$(($results.Successes | ForEach-Object { "- $_" }) -join "`n")

## Issues ❌
$(($results.Issues | ForEach-Object { "- $_" }) -join "`n")

## Action Items
$(if ($percentage -lt 80) {
"* [ ] Move repository interfaces from Infrastructure to Domain
* [ ] Remove database dependencies from unit tests  
* [ ] Use direct projections instead of multiple mappings
* [ ] Consolidate unnecessary interfaces"
} else {
"🎉 Architecture is in good shape! Keep up the good work."
})

## Audit Criteria

### Domain Dependencies (20 points)
* Domain layer should have NO external references
* All dependencies should point inward

### Interface Location (15 points)  
* Interfaces should be in the layer that CONSUMES them
* Repository interfaces belong in Domain, not Infrastructure

### Test Performance (10 points)
* Unit tests should run in milliseconds
* No database or external dependencies in unit tests

### Layer Complexity (15 points)
* Avoid excessive mapping between layers
* Use direct projections where appropriate

### Interface Design (10 points)
* Don't create interfaces 'just in case'
* Each interface should have a clear purpose

## Next Steps

1. **Week 1**: Fix interface locations
2. **Week 2**: Optimize test performance  
3. **Week 3**: Review and consolidate layers
4. **Week 4**: Re-run audit to measure improvements

---
*Generated by Clean Architecture Audit Tool*
*Repository: https://github.com/vivek-baliyan/clean-architecture-performance*
"@

    $reportContent | Out-File -FilePath $reportPath -Encoding UTF8
    Write-Host "`n📝 Report saved to: $reportPath" -ForegroundColor Cyan
}

# Exit with appropriate code
if ($percentage -ge 80) {
    Write-Host "`n🎉 Architecture audit passed!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`n⚠️  Architecture needs improvement." -ForegroundColor Yellow
    exit 1
}
