# Quiz Submission Debugging Guide

## Common Error: "Failed to submit quiz. Please try again."

### Root Causes & Solutions

#### 1. API Endpoint Mismatch

**Check:**
- Backend API URL in `attempt.service.ts`
- Should be: `http://localhost:5137/api/attempts`

**Fix:**
```typescript
// src/app/service/attempt.service.ts
private readonly BASE = 'http://localhost:5137/api/attempts';
```

#### 2. Backend Not Running

**Symptoms:**
- Network error in browser console
- "ERR_CONNECTION_REFUSED"

**Solution:**
```bash
cd QuizAppProject
dotnet run
```

Verify backend is running:
- Open browser: `http://localhost:5137/swagger`
- Should see Swagger UI

#### 3. CORS Issues

**Symptoms:**
- CORS policy error in console
- "Access-Control-Allow-Origin" error

**Already Fixed in Backend:**
```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin();
    });
});

app.UseCors("DefaultCors");
```

#### 4. Missing JWT Token

**Symptoms:**
- 401 Unauthorized error
- User appears logged in but API rejects request

**Debug:**
Open browser console:
```javascript
// Check if token exists
console.log(localStorage.getItem('token'));

// Should return JWT token like: "eyJhbGc..."
```

**Solution:**
- Logout and login again
- Clear localStorage: `localStorage.clear()`
- Register new account

#### 5. Invalid Quiz Data

**Symptoms:**
- 400 Bad Request error
- Validation error from backend

**Common Issues:**
- User ID is null/undefined
- Quiz ID is invalid
- Answers array is empty
- Answer options not "A", "B", "C", or "D"

**Debug in Browser Console:**
```javascript
// In quiz.component.ts, add before submit:
console.log('User ID:', this.auth.currentUser()?.userId);
console.log('Quiz ID:', this.quiz()?.quizId);
console.log('Answers:', this.answers());
```

**Expected Format:**
```json
{
  "quizId": "guid-here",
  "userId": "guid-here",
  "answers": [
    {
      "questionId": "guid-here",
      "chosenOption": "A"
    }
  ],
  "startedAtUtc": "2024-03-21T10:00:00Z",
  "endedAtUtc": "2024-03-21T10:15:00Z"
}
```

#### 6. Database Connection Issues

**Symptoms:**
- 500 Internal Server Error
- Backend logs show SQL errors

**Solution:**
```bash
# Check connection string in appsettings.json
# Re-run migrations
dotnet ef database update

# If migrations fail, try:
dotnet ef database drop
dotnet ef database update
```

## Step-by-Step Debugging

### Step 1: Check Backend
```bash
# Terminal 1 - Start backend
cd QuizAppProject
dotnet run

# Should see:
# Now listening on: http://localhost:5137
```

### Step 2: Test API Directly
Use Swagger UI: `http://localhost:5137/swagger`

1. **Test Auth:**
   - POST `/api/auth/login`
   - Copy the token from response

2. **Test Attempts:**
   - Click "Authorize" button
   - Paste token
   - POST `/api/attempts` with test data

### Step 3: Check Frontend
```bash
# Terminal 2 - Start frontend
cd QuizApp
ng serve --proxy-config proxy.conf.json
```

### Step 4: Browser Developer Tools

**Network Tab:**
1. Open DevTools (F12)
2. Go to Network tab
3. Take a quiz
4. Click Submit
5. Look for `/api/attempts` request

**Check Request:**
- Method: POST
- Status: Should be 200
- Request Payload: Should have userId, quizId, answers

**Check Response:**
- Should have: attemptAnswerId, totalMark, percentage, feedback

**Common Errors:**

| Status | Meaning | Solution |
|--------|---------|----------|
| 400 | Bad Request | Check request data format |
| 401 | Unauthorized | Login again / check JWT |
| 404 | Not Found | Check API URL |
| 500 | Server Error | Check backend logs |

### Step 5: Console Logs

Add debugging to `quiz.component.ts`:

```typescript
submitQuiz() {
  clearInterval(this.timerInterval);
  this.submitting.set(true);
  
  const userId = this.auth.currentUser()?.userId!;
  const quizId = this.quiz()!.quizId;
  
  // DEBUGGING
  console.log('=== SUBMITTING QUIZ ===');
  console.log('User ID:', userId);
  console.log('Quiz ID:', quizId);
  console.log('Current User:', this.auth.currentUser());
  console.log('Quiz Data:', this.quiz());
  
  const answers: AttemptAnswerItemDto[] = this.questions()
    .map((q, i) => ({ 
      questionId: q.questionId, 
      chosenOption: this.answers()[i] || 'A' 
    }));
    
  console.log('Answers:', answers);
  
  const payload = {
    quizId, userId, answers,
    startedAtUtc: this.startedAt,
    endedAtUtc: new Date().toISOString()
  };
  
  console.log('Final Payload:', payload);
  
  this.attemptSvc.submit(payload).subscribe({
    next: (res) => { 
      console.log('SUCCESS:', res);
      this.submitting.set(false); 
      this.router.navigate(['/result', res.attemptAnswerId]); 
    },
    error: (err) => { 
      console.error('ERROR:', err);
      console.error('Error Details:', err.error);
      console.error('Error Status:', err.status);
      this.submitting.set(false); 
      this.error.set('Failed to submit quiz. Please try again.'); 
    }
  });
}
```

## Quick Fix Checklist

✅ Backend running on port 5137
✅ Frontend running on port 4200  
✅ Database migrations applied
✅ User logged in (check localStorage token)
✅ Quiz has questions
✅ All questions have 4 options
✅ Correct option is A, B, C, or D
✅ CORS enabled in backend
✅ JWT token in Authorization header

## Still Not Working?

### Nuclear Option - Fresh Start

**Backend:**
```bash
cd QuizAppProject
dotnet ef database drop -f
dotnet ef database update
dotnet run
```

**Frontend:**
```bash
cd QuizApp
# Clear browser cache and localStorage
# Ctrl+Shift+Delete (Chrome)
# Or in Console: localStorage.clear()

npm start
```

**Register Fresh Accounts:**
1. Creator: creator@test.com / Test@123
2. Taker: taker@test.com / Test@123
3. Create a category
4. Create a quiz
5. Take the quiz

## Testing with Postman/Thunder Client

### 1. Login
```http
POST http://localhost:5137/api/auth/login
Content-Type: application/json

{
  "username": "creator1",
  "password": "Test@123"
}
```

### 2. Create Quiz Attempt
```http
POST http://localhost:5137/api/attempts
Content-Type: application/json
Authorization: Bearer {your-token-here}

{
  "quizId": "{quiz-guid}",
  "userId": "{user-guid}",
  "answers": [
    {
      "questionId": "{question-guid}",
      "chosenOption": "A"
    }
  ],
  "startedAtUtc": "2024-03-21T10:00:00Z",
  "endedAtUtc": "2024-03-21T10:15:00Z"
}
```

## Backend Logs to Check

```bash
cd QuizAppProject
dotnet run

# Watch for:
# - SQL queries
# - Exception messages
# - Stack traces
# - Validation errors
```

Common backend errors:
- "Quiz not found" - Invalid quiz ID
- "User not found" - Invalid user ID
- "Failed to create attempt" - Database error
- "Time limit exceeded" - Took too long

## Success Indicators

When everything works:
1. ✅ Submit button click
2. ✅ Loading spinner appears
3. ✅ Network request shows 200 OK
4. ✅ Response contains attemptAnswerId
5. ✅ Navigates to /result page
6. ✅ Results page shows score
7. ✅ Feedback shows correct/wrong answers
8. ✅ Can click "View Leaderboard"

## Additional Resources

- Backend Swagger: http://localhost:5137/swagger
- Angular DevTools: Install from Chrome Web Store
- Database Tool: SQL Server Management Studio or Azure Data Studio
