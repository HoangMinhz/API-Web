# Check if SQLite is installed
if (-not (Get-Command sqlite3 -ErrorAction SilentlyContinue)) {
    Write-Host "SQLite is not installed. Please install SQLite first."
    exit 1
}

# Path to the database file
$dbPath = "test.db"

# Path to the SQL script
$sqlPath = "seed_data.sql"

# Execute the SQL script
Write-Host "Seeding database..."
sqlite3 $dbPath < $sqlPath

if ($LASTEXITCODE -eq 0) {
    Write-Host "Database seeded successfully!"
} else {
    Write-Host "Error seeding database. Exit code: $LASTEXITCODE"
} 