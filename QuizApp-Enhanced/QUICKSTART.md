# QuizApp - Quick Start Guide

## Prerequisites
- Node.js 18+ 
- .NET 8 SDK
- SQL Server (LocalDB or full version)
- Visual Studio Code (recommended)

## 5-Minute Setup

### 1. Backend Setup (2 minutes)
```bash
# Navigate to backend folder
cd QuizAppProject

# Restore packages
dotnet restore

# Apply database migrations
dotnet ef database update

# Run the backend
dotnet run
```

✅ Backend running at: http://localhost:5137
✅ Swagger UI at: http://localhost:5137/swagger

### 2. Frontend Setup (3 minutes)
```bash
# Open new terminal
# Navigate to frontend folder
cd QuizApp

# Install dependencies (first time only)
npm install

# Start development server
npm start
```

✅ Frontend running at: http://localhost:4200

## First Time Use

### Create Creator Account
1. Open: http://localhost:4200
2. Click "Register"
3. Fill in:
   - Username: `admin`
   - Email: `admin@quiz.com`
   - Password: `Admin@123`
   - Role: `Creator` ⚠️ (Important!)
   - Name: `Admin User`
4. Click Register
5. Login with same credentials

### Add Categories (Creator)
1. On Dashboard, click "🏷️ Manage Categories"
2. Add these categories:
   - Science
   - Mathematics
   - History
   - Sports
   - Technology

### Create Your First Quiz (Creator)
1. Click "+ New Quiz" on Dashboard
2. Fill in:
   - Quiz Name: "Basic Science Quiz"
   - Category: Science
   - Difficulty: Easy
   - Pass Mark: 3
   - Time Limit: 10 minutes
3. Click "Add Question" button
4. Add 5 questions with options
5. Example Question:
   ```
   Question: What is the chemical symbol for water?
   Option A: H2O ✓ (Mark as correct)
   Option B: CO2
   Option C: O2
   Option D: N2
   ```
6. Click "Save Quiz"

### Create Taker Account
1. Logout (top right)
2. Click "Register"
3. Fill in:
   - Username: `student`
   - Email: `student@quiz.com`  
   - Password: `Student@123`
   - Role: `Taker` ⚠️ (Important!)
   - Name: `Student User`
4. Login with taker credentials

### Take a Quiz (Taker)
1. Browse quizzes on Dashboard
2. Click "Play Now" on "Basic Science Quiz"
3. Answer all questions
4. Timer runs automatically
5. Click "Submit Quiz" or wait for timer
6. View your results
7. Check feedback for each question
8. Click "🏆 View Leaderboard"

## Project URLs

| Service | URL | Purpose |
|---------|-----|---------|
| Frontend | http://localhost:4200 | Main application |
| Backend API | http://localhost:5137 | REST API |
| Swagger UI | http://localhost:5137/swagger | API documentation |

## Default Ports

- Frontend (Angular): **4200**
- Backend (.NET): **5137**

Change ports if needed:
- Frontend: `ng serve --port 4300`
- Backend: Edit `launchSettings.json`

## Common Commands

### Backend
```bash
# Run backend
dotnet run

# Run with watch (auto-reload)
dotnet watch run

# Create new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Drop database
dotnet ef database drop
```

### Frontend
```bash
# Start dev server
npm start

# Build for production
npm run build

# Run tests
npm test

# Lint code
npm run lint
```

## File Structure

```
.
├── QuizAppProject/          # .NET Backend
│   ├── Controllers/         # API endpoints
│   ├── Models/             # Data models & DTOs
│   ├── Services/           # Business logic
│   ├── Context/            # Database context
│   └── Program.cs          # Entry point
│
└── QuizApp/                # Angular Frontend
    ├── src/
    │   ├── app/
    │   │   ├── dashboard/  # Main dashboard
    │   │   ├── quiz/       # Quiz taking
    │   │   ├── category/   # Category management
    │   │   ├── attempt/    # Results display
    │   │   └── service/    # HTTP services
    │   └── styles.css
    └── package.json
```

## Environment Configuration

### Backend (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=QuizAppDb;..."
  },
  "Jwt": {
    "Key": "YOUR_SECURE_KEY_HERE_MIN_32_CHARS",
    "Issuer": "QuizApp",
    "Audience": "QuizAppClients",
    "DurationInMinutes": 1440
  }
}
```

### Frontend (proxy.conf.json)
```json
{
  "/api": {
    "target": "http://localhost:5137",
    "secure": false,
    "changeOrigin": true
  }
}
```

## Features Overview

### For Quiz Creators
✅ Create unlimited quizzes
✅ Manage categories
✅ Add questions with 4 options
✅ Set difficulty levels
✅ Configure time limits
✅ Define pass marks
✅ Edit/Delete quizzes

### For Quiz Takers
✅ Browse available quizzes
✅ Filter by category
✅ Take timed quizzes
✅ View instant results
✅ See detailed feedback
✅ Check leaderboard rankings
✅ Track attempt history

## Troubleshooting

### Backend won't start
```bash
# Check .NET version
dotnet --version  # Should be 8.0+

# Restore packages
dotnet restore

# Clean and rebuild
dotnet clean
dotnet build
```

### Frontend won't start
```bash
# Check Node version
node --version  # Should be 18+

# Clear cache
npm cache clean --force

# Reinstall
rm -rf node_modules package-lock.json
npm install
```

### Database issues
```bash
# Reset database
dotnet ef database drop -f
dotnet ef database update
```

### Port already in use
```bash
# Frontend
ng serve --port 4201

# Backend - Edit launchSettings.json
# Change applicationUrl port
```

## Next Steps

1. ✅ Complete setup
2. ✅ Create creator account
3. ✅ Add categories
4. ✅ Create sample quiz
5. ✅ Create taker account
6. ✅ Take the quiz
7. ✅ Check results
8. ✅ View leaderboard

## Support

Having issues? Check:
1. `FIXES_AND_SETUP.md` - Detailed setup
2. `DEBUGGING_GUIDE.md` - Troubleshooting
3. Browser Console (F12) - Frontend errors
4. Terminal - Backend errors
5. Swagger UI - API testing

## Production Deployment

### Backend
```bash
dotnet publish -c Release -o ./publish
```

### Frontend  
```bash
ng build --configuration production
```

Deploy `/dist/quiz-app` folder to web server.

## License & Credits

- Angular 21
- .NET 8
- Bootstrap 5
- Modern responsive design
- JWT Authentication
- Entity Framework Core

---

**You're all set! 🎉**

Open http://localhost:4200 and start creating quizzes!
