# QuizApp - Complete Setup & Fixes Guide

## Issues Fixed

### 1. ✅ Quiz Creators Can Now Manage Categories
- Added **"Manage Categories"** button on dashboard for creators
- Quiz creators can now:
  - Add new categories
  - Edit existing categories
  - Delete categories (if no quizzes are linked)
  - Access via Dashboard → "🏷️ Manage Categories" button

### 2. ✅ Quiz Submission Fixed
- Fixed API endpoint communication
- Proper error handling for quiz submission
- Correct data flow from quiz component to backend
- After submission, users see their score and results

### 3. ✅ Leaderboard Access Improved
- After completing a quiz, users see their results
- Direct "View Leaderboard" button on results page
- Smooth navigation flow: Quiz → Results → Leaderboard

## Setup Instructions

### Backend Setup (.NET 8)

1. **Update Connection String** (if needed)
   - Open `appsettings.json`
   - Ensure SQL Server connection string is correct:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=QuizAppDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
   }
   ```

2. **Run Migrations**
   ```bash
   cd QuizAppProject
   dotnet ef database update
   ```

3. **Start Backend**
   ```bash
   dotnet run
   ```
   - Backend will run on: `http://localhost:5137`
   - Swagger UI: `http://localhost:5137/swagger`

### Frontend Setup (Angular 21)

1. **Install Dependencies**
   ```bash
   cd QuizApp
   npm install
   ```

2. **Start Development Server**
   ```bash
   npm start
   ```
   OR with proxy (recommended):
   ```bash
   ng serve --proxy-config proxy.conf.json
   ```
   - Frontend will run on: `http://localhost:4200`

3. **Build for Production**
   ```bash
   npm run build
   ```

## API Endpoints Reference

### Authentication
- POST `/api/auth/register` - Register new user
- POST `/api/auth/login` - Login user
- POST `/api/auth/forgot-password` - Request password reset
- POST `/api/auth/reset-password` - Reset password

### Categories (Creator Only)
- GET `/api/categories` - Get all categories
- POST `/api/categories` - Create category
- PUT `/api/categories/{id}` - Update category
- DELETE `/api/categories/{id}` - Delete category

### Quizzes
- GET `/api/quizzes` - Get all quizzes
- GET `/api/quizzes/{id}` - Get quiz by ID
- POST `/api/quizzes` - Create quiz (Creator only)
- PUT `/api/quizzes/{id}` - Update quiz (Creator only)
- DELETE `/api/quizzes/{id}` - Delete quiz (Creator only)

### Attempts (Quiz Taking)
- POST `/api/attempts` - Submit quiz attempt
- GET `/api/attempts/mine?userId={id}` - Get user's attempts
- GET `/api/attempts/{attemptId}` - Get attempt details

### Leaderboard
- GET `/api/leaderboard?categoryId={id}&take={count}` - Get top scores

## User Roles

### Quiz Creator
- Create and manage quizzes
- Add/Edit/Delete categories
- Set questions with 4 options (A, B, C, D)
- Define pass marks and time limits
- Choose difficulty levels

### Quiz Taker
- Browse available quizzes
- Take quizzes
- View results and feedback
- Check leaderboard rankings

## Testing the Application

### 1. Register as Creator
```
Username: creator1
Email: creator@test.com
Password: Test@123
Role: Creator
```

### 2. Create Categories
- Login as creator
- Go to Dashboard
- Click "🏷️ Manage Categories"
- Add categories like:
  - Science
  - Mathematics
  - History
  - General Knowledge

### 3. Create Quiz
- Click "+ New Quiz" on Dashboard
- Fill in quiz details:
  - Quiz Name
  - Select Category
  - Difficulty Level
  - Pass Mark
  - Time Limit (optional)
- Add Questions:
  - Click "Add Question"
  - Enter question text
  - Fill options A, B, C, D
  - Select correct option
- Click "Save Quiz"

### 4. Register as Taker
```
Username: taker1
Email: taker@test.com
Password: Test@123
Role: Taker
```

### 5. Take Quiz
- Login as taker
- Browse quizzes on Dashboard
- Click "Play Now" on any quiz
- Answer questions (timer runs for each question)
- Submit quiz

### 6. View Results
- After submission, see:
  - Total Score
  - Percentage
  - Correct/Wrong breakdown
  - Question-by-question feedback
- Click "🏆 View Leaderboard"

## Common Issues & Solutions

### Issue: Cannot connect to API
**Solution:** 
- Ensure backend is running on port 5137
- Check `proxy.conf.json` configuration
- Start Angular with: `ng serve --proxy-config proxy.conf.json`

### Issue: "Failed to submit quiz"
**Solution:**
- Check browser console for errors
- Verify user is logged in (check JWT token in localStorage)
- Ensure all questions have selected answers
- Check backend logs for errors

### Issue: Categories not showing for creators
**Solution:**
- Verify user role is "Creator" (case-sensitive)
- Check `AuthService.isCreator()` method
- Ensure JWT token contains correct role claim

### Issue: CORS errors
**Solution:**
Backend already has CORS configured. If issues persist:
```csharp
// In Program.cs - already added:
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin();
    });
});
```

## Project Structure

```
QuizApp/
├── src/
│   ├── app/
│   │   ├── attempt/          # Quiz results component
│   │   ├── category/         # Category management (Creator)
│   │   ├── dashboard/        # Main dashboard
│   │   ├── guards/           # Auth & role guards
│   │   ├── interceptor/      # JWT interceptor
│   │   ├── leaderboard/      # Leaderboard component
│   │   ├── login/            # Login component
│   │   ├── models/           # TypeScript interfaces
│   │   ├── question/         # Quiz creation (Creator)
│   │   ├── quiz/             # Quiz taking component
│   │   ├── register/         # Registration component
│   │   ├── service/          # HTTP services
│   │   └── ...
│   ├── index.html
│   ├── main.ts
│   └── styles.css
├── proxy.conf.json
├── angular.json
└── package.json
```

## Features Implemented

✅ User Authentication (Register/Login)
✅ Role-based Access (Creator/Taker)
✅ Category Management (Creator)
✅ Quiz Creation with Questions (Creator)
✅ Quiz Taking with Timer
✅ Automatic Grading
✅ Results with Feedback
✅ Leaderboard System
✅ Responsive Design
✅ Modern UI with Animations

## Environment Variables

### Backend (appsettings.json)
```json
{
  "Jwt": {
    "Key": "YOUR_SECRET_KEY_HERE_AT_LEAST_32_CHARS",
    "Issuer": "QuizApp",
    "Audience": "QuizAppClients",
    "DurationInMinutes": 1440
  }
}
```

### Frontend (Services)
- API Base URL: `http://localhost:5137/api`
- Can be changed in service files if needed

## Browser Support
- Chrome (recommended)
- Firefox
- Edge
- Safari

## Database Schema

- **Users** - User accounts
- **UserDetails** - Extended user information
- **Categories** - Quiz categories
- **Quizzes** - Quiz definitions
- **Questions** - Quiz questions
- **Options** - Question options (A, B, C, D)
- **AttemptAnswers** - User quiz attempts and scores

## Support

For issues or questions:
1. Check browser console for errors
2. Check backend logs
3. Verify database migrations are up to date
4. Ensure all dependencies are installed

## Version Info
- Angular: 21.x
- .NET: 8.0
- Node.js: 18.x or higher recommended
- TypeScript: 5.x
