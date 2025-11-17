# PowerShell Script to Add i18n Attributes to All HTML Pages
# This script adds data-i18n attributes to common elements

$projectRoot = "F:\Work\barq-tms\final-barq\barq-dashboard"
$pagesDir = Join-Path $projectRoot "frontend\pages"

# Navigation translations mapping
$navTranslations = @{
    'Dashboard' = 'nav.dashboard'
    'Tasks' = 'nav.tasks'
    'Projects' = 'nav.projects'
    'Employees' = 'nav.team'
    'Team Members' = 'nav.team'
    'My Tasks' = 'tasks.myTasks'
    'Team Tasks' = 'nav.tasks'
    'Clients' = 'nav.clients'
    'Calendar' = 'nav.calendar'
    'Analytics' = 'nav.analytics'
    'Settings' = 'nav.settings'
    'Logout' = 'auth.logout'
    'Profile' = 'settings.profile'
}

# Common text translations
$commonTranslations = @{
    'Search...' = 'common.search'
    'New Task' = 'tasks.createTask'
    'New Project' = 'projects.createProject'
    'Total Tasks' = 'dashboard.totalTasks'
    'Active Tasks' = 'dashboard.activeTasks'
    'Completed Tasks' = 'dashboard.completedTasks'
    'Overdue Tasks' = 'dashboard.overdueTasks'
    'Active Projects' = 'dashboard.activeProjects'
    'Total Projects' = 'dashboard.totalProjects'
    'Team Members' = 'dashboard.teamMembers'
    'Total Clients' = 'nav.clients'
    'Recent Tasks' = 'dashboard.recentActivity'
    'Recent Activity' = 'dashboard.recentActivity'
    'Dashboard Overview' = 'dashboard.title'
}

# Table header translations
$tableHeaders = @{
    'Task' = 'tasks.taskTitle'
    'Task Title' = 'tasks.taskTitle'
    'Description' = 'tasks.description'
    'Status' = 'tasks.status'
    'Priority' = 'tasks.priority'
    'Assigned To' = 'tasks.assignee'
    'Assignee' = 'tasks.assignee'
    'Due Date' = 'tasks.dueDate'
    'Start Date' = 'tasks.startDate'
    'Project' = 'tasks.project'
    'Project Name' = 'projects.projectName'
    'Client' = 'projects.client'
    'Budget' = 'projects.budget'
    'End Date' = 'projects.endDate'
    'Progress' = 'tasks.progress'
    'Actions' = 'common.actions'
}

function Add-I18nAttribute {
    param(
        [string]$FilePath
    )
    
    Write-Host "Processing: $FilePath" -ForegroundColor Cyan
    
    $content = Get-Content $FilePath -Raw -Encoding UTF8
    $modified = $false
    
    # Add i18n to navigation items
    foreach ($text in $navTranslations.Keys) {
        $key = $navTranslations[$text]
        $pattern = "(<span class=`"nav-item-text`">)$text(</span>)"
        $replacement = "`$1$text`$2"
        if ($content -match $pattern -and $content -notmatch "data-i18n=`"$key`"") {
            $content = $content -replace $pattern, "<span class=`"nav-item-text`" data-i18n=`"$key`">$text</span>"
            $modified = $true
        }
    }
    
    # Add i18n to search placeholder
    if ($content -match 'placeholder="Search\.\.\."' -and $content -notmatch 'data-i18n-placeholder="common.search"') {
        $content = $content -replace 'placeholder="Search\.\.\."', 'data-i18n-placeholder="common.search" placeholder="Search..."'
        $modified = $true
    }
    
    # Add i18n to common stat labels
    foreach ($text in $commonTranslations.Keys) {
        $key = $commonTranslations[$text]
        $pattern = "(<span class=`"stat-label`">)$text(</span>)"
        if ($content -match $pattern -and $content -notmatch "data-i18n=`"$key`"") {
            $content = $content -replace $pattern, "<span class=`"stat-label`" data-i18n=`"$key`">$text</span>"
            $modified = $true
        }
    }
    
    # Add i18n to table headers
    foreach ($text in $tableHeaders.Keys) {
        $key = $tableHeaders[$text]
        $pattern = "(<th>)$text(</th>)"
        if ($content -match $pattern -and $content -notmatch "data-i18n=`"$key`"") {
            $content = $content -replace $pattern, "<th data-i18n=`"$key`">$text</th>"
            $modified = $true
        }
    }
    
    # Add i18n to headings with common patterns
    if ($content -match '<h2>Dashboard Overview</h2>' -and $content -notmatch 'data-i18n="dashboard.title"') {
        $content = $content -replace '<h2>Dashboard Overview</h2>', '<h2 data-i18n="dashboard.title">Dashboard Overview</h2>'
        $modified = $true
    }
    
    # Add number class to stat values if not present
    $content = $content -replace '(<span class="stat-value")', '<span class="stat-value number"'
    
    if ($modified) {
        Set-Content $FilePath -Value $content -Encoding UTF8 -NoNewline
        Write-Host "  ✓ Updated" -ForegroundColor Green
        return $true
    } else {
        Write-Host "  - No changes needed" -ForegroundColor Gray
        return $false
    }
}

# Process all HTML files
$allFiles = Get-ChildItem -Path $pagesDir -Filter "*.html" -Recurse -Exclude "i18n-example.html","my-team.html"
$updatedCount = 0

foreach ($file in $allFiles) {
    if (Add-I18nAttribute -FilePath $file.FullName) {
        $updatedCount++
    }
}

Write-Host "" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow
Write-Host "Summary: Updated $updatedCount of $($allFiles.Count) files" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow
