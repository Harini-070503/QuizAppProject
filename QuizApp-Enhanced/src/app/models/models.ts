// ===== CORE MODELS =====

export interface User {
  userId: string;
  username: string;
  email: string;
  passwordHash?: string;
  salt?: string;
  role: string;
  createdAt?: string;
  userDetails?: UserDetails;
}

export interface UserDetails {
  userDetailsId?: string;
  userId?: string;
  phoneNumber?: string;
  addressLine1?: string;
  addressLine2?: string;
  state?: string;
  city?: string;
  pincode?: string;
  name?: string;
  createdAt?: string;
}

export interface Quiz {
  quizId: string;
  userId?: string;
  categoryId?: string;
  quizName: string;
  description?: string;
  passMark: number;
  totalQuestion: number;
  difficultyLevel?: string;
  timeLimit?: number;
  createdAt?: string;
  category?: Category;
  questions?: Question[];
}

export interface Question {
  questionId: string;
  quizId?: string;
  questionText: string;
  createdAt?: string;
  options?: Option;
}

export interface Option {
  optionId?: string;
  questionId?: string;
  optionA: string;
  optionB: string;
  optionC?: string;  // Made optional for 2-3 option questions
  optionD?: string;  // Made optional for 2-3 option questions
  correctOption?: string;
  createdAt?: string;
}

export interface Category {
  categoryId: string;
  categoryName: string;
  createdAt?: string;
}

export interface AttemptAnswer {
  attemptAnswerId?: string;
  userId?: string;
  quizId?: string;
  totalMark?: number;
  percentage?: number;
  createdAt?: string;
}

// ===== DTOs =====

export interface AuthDto {
  username: string;
  email: string;
  password: string;
  role: string;
  name?: string;
}

export interface UserDto {
  userId: string;
  username: string;
  email: string;
  role: string;
  name?: string;
  phoneNumber?: string;
  addressLine1?: string;
  addressLine2?: string;
  state?: string;
  city?: string;
  pincode?: string;
}

export interface UpdateUserDto {
  name?: string;
  phoneNumber?: string;
  addressLine1?: string;
  addressLine2?: string;
  state?: string;
  city?: string;
  pincode?: string;
}

export interface QuizDto {
  quizId: string;
  quizName: string;
  description?: string;
  difficultyLevel?: string;
  timeLimit?: number;
  deadline?: string;
  isExpired?: boolean;
  groupId?: string;
  passMark: number;
  totalQuestion: number;
  category?: CategoryDto;
  creatorId?: string;
  creatorName?: string;
  creatorRole?: string;
  questions?: QuestionDto[];
}

export interface QuizCreateDto {
  quizName: string;
  description?: string;
  categoryId: string;
  passMark: number;
  totalQuestion: number;
  difficultyLevel?: string;
  timeLimit?: number;
  deadline?: string;
  questions?: QuestionCreateDto[];
}

export interface QuizUpdateDto {
  quizName: string;
  description?: string;
  categoryId: string;
  passMark: number;
  totalQuestion: number;
  difficultyLevel?: string;
  timeLimit?: number;
  deadline?: string;
  questions?: QuestionCreateDto[];
}

export interface QuestionDto {
  questionId: string;
  questionText: string;
  marks: number;
  options?: OptionDto;
}

export interface QuestionCreateDto {
  questionText: string;
  marks: number;
  options: OptionCreateDto;
  correctOptions?: string[];
}

export interface OptionDto {
  optionId?: string;
  optionA: string;
  optionB: string;
  optionC?: string;  // Optional for 2-3 option questions
  optionD?: string;  // Optional for 2-3 option questions
  correctOption?: string;
}

export interface OptionCreateDto {
  optionA: string;
  optionB: string;
  optionC?: string;
  optionD?: string;
  correctOption: string; // single "A" or comma-separated "A,C"
}

export interface CategoryDto {
  categoryId: string;
  categoryName: string;
}

export interface CategoryCreateDto {
  categoryName: string;
}

export interface AttemptDto {
  quizId: string;
  userId: string;
  answers: AttemptAnswerItemDto[];
  startedAtUtc?: string;
  endedAtUtc?: string;
}

export interface AttemptAnswerItemDto {
  questionId: string;
  chosenOption: string;
}

export interface AttemptResultDto {
  attemptAnswerId: string;
  quizId: string;
  totalMark: number;
  percentage: number;
  isPendingEvaluation?: boolean;
  feedback: AttemptFeedbackItemDto[];
}

export interface AttemptFeedbackItemDto {
  questionId: string;
  correctOption: string;
  yourOption?: string;
  isCorrect: boolean;
}

export interface LeaderboardDto {
  userId: string;
  username: string;
  name?: string;
  quizId: string;
  quizName: string;
  percentage: number;
  createdAt: string;
}

export interface LoginRequestDto {
  username: string;
  password: string;
}

export interface RegisterRequestDto {
  username: string;
  email: string;
  password: string;
  role: string;
  name?: string;
}

export interface AuthResponseDto {
  token: string;
}

export interface ForgotPasswordRequestDto {
  usernameOrEmail: string;
}

export interface ForgotPasswordResponseDto {
  resetToken: string;
  expiresAtUtc: string;
}

export interface ResetPasswordRequestDto {
  username: string;
  resetToken: string;
  newPassword: string;
}

// ===== EVALUATOR =====

export interface SubmissionDetailDto {
  attemptAnswerId: string;
  quizId: string;
  quizName: string;
  userId: string;
  username: string;
  totalMark: number;
  maxMark: number;
  percentage: number;
  submittedAt: string;
  answers: SubmissionAnswerDto[];
}

export interface SubmissionAnswerDto {
  questionId: string;
  questionText: string;
  chosenOption?: string;
  correctOption: string;
  isCorrect: boolean;
  marksAwarded: number;
  maxMarks: number;
}

export interface UpdateScoreDto {
  newTotalMark: number;
  questionScores: { questionId: string; marksAwarded: number }[];
}

// ===== GROUPS =====

export interface GroupDto {
  groupId: string;
  groupName: string;
  description?: string;
  evaluatorId: string;
  createdAt: string;
  memberCount: number;
  members: GroupMemberDto[];
}

export interface GroupMemberDto {
  userId: string;
  username: string;
  email: string;
  addedAt: string;
}

export interface GroupCreateDto {
  groupName: string;
  description?: string;
}
