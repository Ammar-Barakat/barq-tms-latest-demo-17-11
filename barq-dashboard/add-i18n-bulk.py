import os
import re
from pathlib import Path

# Stat label mappings
stat_replacements = {
    'My Tasks': 'tasks.myTasks',
    'Pending Tasks': 'tasks.pending',
    'Completed Tasks': 'tasks.completed',
    'My Projects': 'projects.myProjects',
    'Total Tasks': 'dashboard.totalTasks',
    'Active Projects': 'dashboard.activeProjects',
    'Team Members': 'dashboard.teamMembers',
    'Pending Approvals': 'dashboard.pendingApprovals',
    'Total Projects': 'dashboard.totalProjects',
    'Total Clients': 'dashboard.totalClients',
    'Active Tasks': 'tasks.active',
    'Overdue Tasks': 'tasks.overdue',
    'Total Employees': 'dashboard.totalEmployees',
    'Revenue': 'dashboard.revenue',
    'Tasks Assigned': 'tasks.assigned',
    'Projects Created': 'projects.created',
}

# Table header mappings
table_headers = {
    'Task': 'tasks.taskTitle',
    'Task Title': 'tasks.taskTitle',
    'Project': 'tasks.project',
    'Project Name': 'projects.projectName',
    'Assigned To': 'tasks.assignee',
    'Assignee': 'tasks.assignee',
    'Status': 'tasks.status',
    'Priority': 'tasks.priority',
    'Due Date': 'tasks.dueDate',
    'Client': 'projects.client',
    'Budget': 'projects.budget',
    'Start Date': 'projects.startDate',
    'End Date': 'projects.endDate',
    'Progress': 'projects.progress',
    'Actions': 'common.actions',
    'Name': 'common.name',
    'Email': 'common.email',
    'Role': 'common.role',
    'Phone': 'common.phone',
    'Department': 'common.department',
}

# Button mappings
button_texts = {
    'Create Task': 'tasks.createTask',
    'Create Project': 'projects.createProject',
    'Add Employee': 'common.addEmployee',
    'Add Client': 'common.addClient',
    'View All': 'common.viewAll',
    'View Details': 'common.viewDetails',
    'Export': 'common.export',
}

def process_file(file_path):
    """Process a single HTML file to add i18n attributes."""
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original_content = content
    modified = False
    
    # Process stat labels
    for label, key in stat_replacements.items():
        pattern = f'<span class="stat-label">{re.escape(label)}</span>'
        replacement = f'<span class="stat-label" data-i18n="{key}">{label}</span>'
        if re.search(pattern, content) and f'data-i18n="{key}"' not in content:
            content = re.sub(pattern, replacement, content)
            modified = True
    
    # Process table headers
    for header, key in table_headers.items():
        pattern = f'<th>{re.escape(header)}</th>'
        replacement = f'<th data-i18n="{key}">{header}</th>'
        if re.search(pattern, content) and f'data-i18n="{key}"' not in content:
            content = re.sub(pattern, replacement, content)
            modified = True
    
    # Process buttons
    for text, key in button_texts.items():
        # Button pattern
        pattern = f'<button[^>]*>\\s*<i[^>]*></i>\\s*{re.escape(text)}\\s*</button>'
        if re.search(pattern, content) and f'data-i18n="{key}"' not in content:
            # More complex replacement to preserve button attributes
            content = re.sub(
                f'(<button[^>]*>\\s*<i[^>]*></i>\\s*){re.escape(text)}(\\s*</button>)',
                f'\\1<span data-i18n="{key}">{text}</span>\\2',
                content
            )
            modified = True
        
        # Simple button pattern without icon
        pattern2 = f'<button[^>]*>{re.escape(text)}</button>'
        if re.search(pattern2, content) and f'data-i18n="{key}"' not in content:
            content = re.sub(
                f'(<button[^>]*>){re.escape(text)}(</button>)',
                f'\\1<span data-i18n="{key}">{text}</span>\\2',
                content
            )
            modified = True
    
    if modified and content != original_content:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(content)
        return True
    return False

def main():
    """Process all HTML files in frontend/pages directory."""
    base_dir = Path(r'F:\Work\barq-tms\final-barq\barq-dashboard\frontend\pages')
    
    exclude_files = {'i18n-example.html', 'my-team.html', 'login.html'}
    updated_files = []
    
    for html_file in base_dir.rglob('*.html'):
        if html_file.name not in exclude_files:
            if process_file(html_file):
                updated_files.append(html_file.name)
                print(f'✓ Updated: {html_file.name}')
    
    print(f'\n{len(updated_files)} files updated successfully!')

if __name__ == '__main__':
    main()
