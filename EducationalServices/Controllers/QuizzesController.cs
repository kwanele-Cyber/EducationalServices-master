using EducationalServices.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Linq.Expressions;
using Microsoft.AspNet.Identity.EntityFramework;


namespace EducationalServices.Controllers
{
    // EducationalServices, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
    // EducationalServices.Controllers.QuizzesController
    

    public class QuizzesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "Admin,Tutor")]
        public ActionResult Create()
        {
            string userId = base.User.Identity.GetUserId();
            IQueryable<Module> modules = ((!base.User.IsInRole("Admin")) ? (from tm in db.TutorModules
                                                                            where tm.TutorId == userId
                                                                            select tm.Module) : db.Modules);
            base.ViewBag.ModuleId = new SelectList(modules, "ModuleId", "Subject");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Tutor")]
        public ActionResult Create([Bind(Include = "ModuleId,Title,Description,MaxAttempts,TimeLimit")] Quiz quiz, string StartDate, string StartTime, string EndDate, string EndTime, List<Question> Questions)
        {
            try
            {
                if (base.ModelState.IsValid)
                {
                    quiz.StartTime = CombineDateAndTime(StartDate, StartTime);
                    quiz.EndTime = CombineDateAndTime(EndDate, EndTime);
                    quiz.IsCompleted = false;
                    quiz.Attempts = new List<QuizAttempt>();
                    string userId = base.User.Identity.GetUserId();
                    if (!base.User.IsInRole("Admin") && !db.TutorModules.Any((TutorModule tm) => tm.TutorId == userId && tm.ModuleId == quiz.ModuleId))
                    {
                        base.ModelState.AddModelError("", "You don't have permission to create a quiz for this module.");
                        PrepareModuleDropdown(userId);
                        return View(quiz);
                    }
                    db.Quizzes.Add(quiz);
                    db.SaveChanges();
                    if (Questions != null && Questions.Any())
                    {
                        foreach (Question question in Questions)
                        {
                            question.QuizId = quiz.QuizId;
                            db.Questions.Add(question);
                        }
                    }
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                base.ModelState.AddModelError("", "Error creating quiz: " + ex.Message);
            }
            PrepareModuleDropdown(base.User.Identity.GetUserId());
            return View(quiz);
        }

        private void PrepareModuleDropdown(string userId)
        {
            IQueryable<Module> modules = ((!base.User.IsInRole("Admin")) ? (from tm in db.TutorModules
                                                                            where tm.TutorId == userId
                                                                            select tm.Module) : db.Modules);
            base.ViewBag.ModuleId = new SelectList(modules, "ModuleId", "Subject");
        }

        private DateTime CombineDateAndTime(string date, string time)
        {
            return DateTime.Parse(date + " " + time);
        }

        [Authorize(Roles = "Admin,Tutor")]
        public ActionResult AddQuestions(int id)
        {
            Quiz quiz = db.Quizzes.Find(id);
            if (quiz == null)
            {
                return HttpNotFound();
            }
            base.ViewBag.QuestionTypes = Enum.GetValues(typeof(QuestionType)).Cast<QuestionType>().Select(delegate (QuestionType q)
            {
                SelectListItem obj = new SelectListItem
                {
                    Text = q.ToString()
                };
                int num = (int)q;
                obj.Value = num.ToString();
                return obj;
            });
            return View(quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Tutor")]
        public ActionResult AddQuestions(int QuizId, List<Question> questions)
        {
            Quiz quiz = db.Quizzes.Find(QuizId);
            if (quiz == null)
            {
                return HttpNotFound();
            }
            if (questions != null && questions.Any())
            {
                foreach (Question question in questions)
                {
                    if (string.IsNullOrWhiteSpace(question.Text))
                    {
                        base.ModelState.AddModelError("", "Question text cannot be empty.");
                        return View(quiz);
                    }
                    question.QuizId = QuizId;
                    db.Questions.Add(question);
                }
                db.SaveChanges();
            }
            return RedirectToAction("Details", new
            {
                id = QuizId
            });
        }

        [HttpGet]
        public ActionResult Take(int id)
        {
            Quiz quiz = db.Quizzes.Include((Quiz q) => q.Questions).FirstOrDefault((Quiz q) => q.QuizId == id);
            if (quiz == null)
            {
                return HttpNotFound();
            }
            DateTime now = DateTime.Now;
            if (now < quiz.StartTime || now > quiz.EndTime)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "This quiz is not currently available.");
            }
            string userId = base.User.Identity.GetUserId();
            int attemptCount = db.QuizAttempts.Count((QuizAttempt qa) => qa.QuizId == id && qa.StudentId == userId);
            if (attemptCount >= quiz.MaxAttempts)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "You have reached the maximum number of attempts for this quiz.");
            }
            QuizAttempt attempt = new QuizAttempt
            {
                QuizId = id,
                StudentId = userId,
                StartTime = now,
                IsCompleted = false
            };
            db.QuizAttempts.Add(attempt);
            db.SaveChanges();
            base.ViewBag.AttemptId = attempt.AttemptId;
            return View(quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Take(int id, FormCollection formCollection)
        {
            Quiz quiz = db.Quizzes.Include((Quiz q) => q.Questions).FirstOrDefault((Quiz q) => q.QuizId == id);
            if (quiz == null)
            {
                return HttpNotFound();
            }
            string userId = base.User.Identity.GetUserId();
            DateTime now = DateTime.Now;
            if (now < quiz.StartTime || now > quiz.EndTime)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "This quiz is not currently available.");
            }
            int attemptCount = db.QuizAttempts.Count((QuizAttempt qa) => qa.QuizId == id && qa.StudentId == userId);
            if (attemptCount >= quiz.MaxAttempts)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "You have reached the maximum number of attempts for this quiz.");
            }
            QuizAttempt attempt = new QuizAttempt
            {
                QuizId = id,
                StudentId = userId,
                StartTime = now,
                EndTime = now,
                IsCompleted = true,
                Answers = new List<StudentAnswer>()
            };
            db.QuizAttempts.Add(attempt);
            db.SaveChanges();
            int totalScore = 0;
            foreach (Question question in quiz.Questions)
            {
                string answer = "";
                switch (question.QuestionType)
                {
                    case QuestionType.MultipleChoice:
                    case QuestionType.TrueFalse:
                    case QuestionType.FillInTheBlank:
                    case QuestionType.ShortAnswer:
                    case QuestionType.Essay:
                        answer = formCollection[question.QuestionId.ToString()];
                        break;
                    case QuestionType.MultipleAnswers:
                        {
                            string[] selectedAnswers = formCollection.GetValues(question.QuestionId.ToString());
                            answer = ((selectedAnswers != null) ? string.Join(",", selectedAnswers) : "");
                            break;
                        }
                    case QuestionType.Matching:
                        {
                            List<string> matchingAnswers = new List<string>();
                            for (int i = 0; i < question.Options.Split(';').Length; i++)
                            {
                                string key = $"{question.QuestionId}_{i}";
                                string value = formCollection[key];
                                if (!string.IsNullOrEmpty(value))
                                {
                                    matchingAnswers.Add($"{i}:{value}");
                                }
                            }
                            answer = string.Join(";", matchingAnswers);
                            break;
                        }
                    case QuestionType.Ranking:
                        {
                            List<string> rankingAnswers = new List<string>();
                            for (int i = 0; i < question.Options.Split(',').Length; i++)
                            {
                                string key = $"{question.QuestionId}_{i}";
                                string value = formCollection[key];
                                if (!string.IsNullOrEmpty(value))
                                {
                                    rankingAnswers.Add(value);
                                }
                            }
                            answer = string.Join(",", rankingAnswers);
                            break;
                        }
                }
                StudentAnswer studentAnswer = new StudentAnswer
                {
                    QuizAttemptId = attempt.AttemptId,
                    QuestionId = question.QuestionId,
                    Answer = answer
                };
                bool isCorrect = IsAnswerCorrect(question, answer);
                studentAnswer.MarksObtained = (isCorrect ? question.Marks : 0);
                totalScore += studentAnswer.MarksObtained;
                attempt.Answers.Add(studentAnswer);
            }
            attempt.Score = totalScore;
            attempt.EndTime = DateTime.Now;
            if (quiz.TimeLimit > 0 && (attempt.EndTime.Value - attempt.StartTime).TotalMinutes > (double)quiz.TimeLimit)
            {
                attempt.IsCompleted = false;
                attempt.Score = 0;
            }
            db.SaveChanges();
            return RedirectToAction("Results", new
            {
                quizId = id
            });
        }

        private bool IsAnswerCorrect(Question question, string answer)
        {
            switch (question.QuestionType)
            {
                case QuestionType.MultipleChoice:
                case QuestionType.TrueFalse:
                case QuestionType.FillInTheBlank:
                case QuestionType.ShortAnswer:
                    return string.Equals(answer, question.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
                case QuestionType.Essay:
                    return true;
                case QuestionType.MultipleAnswers:
                    {
                        HashSet<string> correctAnswers = (from a in question.CorrectAnswer.Split(',')
                                                          select a.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                        IEnumerable<string> providedAnswers = from a in answer.Split(',')
                                                              select a.Trim();
                        if (providedAnswers.All((string a) => correctAnswers.Contains(a)))
                        {
                            return providedAnswers.Count() == correctAnswers.Count;
                        }
                        return false;
                    }
                case QuestionType.Matching:
                    return IsMatchingAnswerCorrect(question.CorrectAnswer, answer);
                case QuestionType.Ranking:
                    return IsRankingAnswerCorrect(question.CorrectAnswer, answer);
                default:
                    return false;
            }
        }

        private bool IsMatchingAnswerCorrect(string correctAnswer, string studentAnswer)
        {
            IEnumerable<string[]> correctPairs = from p in correctAnswer.Split(';')
                                                 select p.Split(':');
            IEnumerable<string[]> studentPairs = from p in studentAnswer.Split(';')
                                                 select p.Split(':');
            if (correctPairs.Count() != studentPairs.Count())
            {
                return false;
            }
            foreach (string[] correctPair in correctPairs)
            {
                if (correctPair.Length == 2)
                {
                    string[] matchingStudentPair = studentPairs.FirstOrDefault((string[] sp) => sp.Length == 2 && string.Equals(sp[0].Trim(), correctPair[0].Trim(), StringComparison.OrdinalIgnoreCase));
                    if (matchingStudentPair == null || !string.Equals(matchingStudentPair[1].Trim(), correctPair[1].Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool IsRankingAnswerCorrect(string correctAnswer, string studentAnswer)
        {
            IEnumerable<string> correctOrder = from i in correctAnswer.Split(',')
                                               select i.Trim();
            IEnumerable<string> studentOrder = from i in studentAnswer.Split(',')
                                               select i.Trim();
            return correctOrder.SequenceEqual(studentOrder, StringComparer.OrdinalIgnoreCase);
        }

        [Authorize]
        public ActionResult Results(int? quizId)
        {
            if (!quizId.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Quiz quiz = db.Quizzes.Include((Quiz q) => q.Module).FirstOrDefault((Quiz q) => q.QuizId == ((int?)quizId).Value);
            if (quiz == null)
            {
                return HttpNotFound();
            }
            string currentUserId = base.User.Identity.GetUserId();
            bool isAdminOrTutor = base.User.IsInRole("Admin") || base.User.IsInRole("Tutor");
            IQueryable<QuizAttempt> attemptsQuery = from qa in db.QuizAttempts.Include((QuizAttempt qa) => qa.Student).Include((QuizAttempt qa) => qa.QuizRating)
                                                    where (int?)qa.QuizId == quizId
                                                    select qa;
            if (!isAdminOrTutor)
            {
                attemptsQuery = attemptsQuery.Where((QuizAttempt qa) => qa.StudentId == currentUserId);
            }
            List<QuizAttempt> allAttempts = attemptsQuery.ToList();
            QuizResultsViewModel viewModel = new QuizResultsViewModel
            {
                Quiz = quiz,
                StudentsWhoCompleted = allAttempts.Where((QuizAttempt a) => a.IsCompleted).ToList(),
                StudentsStillTaking = allAttempts.Where((QuizAttempt a) => !a.IsCompleted).ToList(),
                IsAdminOrTutor = isAdminOrTutor
            };
            if (isAdminOrTutor)
            {
                List<string> allStudentIds = (from u in db.Users
                                              where u.Roles.Any((IdentityUserRole r) => r.RoleId == db.Roles.FirstOrDefault((IdentityRole role) => role.Name == "Student").Id)
                                              select u.Id).ToList();
                List<string> attemptedStudentIds = allAttempts.Select((QuizAttempt a) => a.StudentId).Distinct().ToList();
                List<string> notAttemptedStudentIds = allStudentIds.Except(attemptedStudentIds).ToList();
                viewModel.StudentsNotAttempted = db.Users.Where((ApplicationUser u) => notAttemptedStudentIds.Contains(u.Id)).ToList();
            }
            return View(viewModel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [Authorize(Roles = "Admin,Tutor")]
        public ActionResult Index(string searchString)
        {
            IQueryable<Quiz> quizzes = db.Quizzes.Include((Quiz q) => q.Module);
            if (!string.IsNullOrEmpty(searchString))
            {
                quizzes = quizzes.Where((Quiz q) => q.Title.Contains(searchString));
            }
            return View(quizzes.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Quiz quiz = db.Quizzes.Find(id);
            if (quiz == null)
            {
                return HttpNotFound();
            }
            return View(quiz);
        }

        [Authorize(Roles = "Admin,Tutor")]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Quiz quiz = db.Quizzes.Find(id);
            if (quiz == null)
            {
                return HttpNotFound();
            }
            PrepareModuleDropdown(base.User.Identity.GetUserId());
            return View(quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Tutor")]
        public ActionResult Edit([Bind(Include = "QuizId,ModuleId,Title,Description,StartTime,EndTime,MaxAttempts,TimeLimit")] Quiz quiz)
        {
            if (base.ModelState.IsValid)
            {
                db.Entry(quiz).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            PrepareModuleDropdown(base.User.Identity.GetUserId());
            return View(quiz);
        }

        [Authorize]
        public ActionResult Rate(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Quiz quiz = db.Quizzes.Find(id);
            if (quiz == null)
            {
                return HttpNotFound();
            }
            string userId = base.User.Identity.GetUserId();
            QuizRating existingRating = db.QuizRatings.FirstOrDefault((QuizRating r) => (int?)r.QuizId == id && r.StudentId == userId);
            if (existingRating != null)
            {
                return View(existingRating);
            }
            return View(new QuizRating
            {
                QuizId = quiz.QuizId
            });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Rate([Bind(Include = "QuizId,Rating,Comment")] QuizRating quizRating)
        {
            if (base.ModelState.IsValid)
            {
                string userId = base.User.Identity.GetUserId();
                QuizRating existingRating = db.QuizRatings.FirstOrDefault((QuizRating r) => r.QuizId == quizRating.QuizId && r.StudentId == userId);
                if (existingRating != null)
                {
                    existingRating.Rating = quizRating.Rating;
                    existingRating.Comment = quizRating.Comment;
                    existingRating.DateRated = DateTime.Now;
                }
                else
                {
                    quizRating.StudentId = userId;
                    quizRating.DateRated = DateTime.Now;
                    db.QuizRatings.Add(quizRating);
                }
                db.SaveChanges();
                return RedirectToAction("Details", new
                {
                    id = quizRating.QuizId
                });
            }
            return View(quizRating);
        }

        [Authorize(Roles = "Admin,Tutor")]
        public ActionResult Statistics(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Quiz quiz = db.Quizzes.Find(id);
            if (quiz == null)
            {
                return HttpNotFound();
            }
            QuizStatisticsViewModel quizStatisticsViewModel = new QuizStatisticsViewModel();
            quizStatisticsViewModel.Quiz = quiz;
            quizStatisticsViewModel.TotalAttempts = db.QuizAttempts.Count((QuizAttempt a) => (int?)a.QuizId == id);
            quizStatisticsViewModel.AverageScore = db.QuizAttempts.Where((QuizAttempt a) => (int?)a.QuizId == id).Average((QuizAttempt a) => a.Score);
            quizStatisticsViewModel.HighestScore = db.QuizAttempts.Where((QuizAttempt a) => (int?)a.QuizId == id).Max((QuizAttempt a) => a.Score);
            quizStatisticsViewModel.LowestScore = db.QuizAttempts.Where((QuizAttempt a) => (int?)a.QuizId == id).Min((QuizAttempt a) => a.Score);
            quizStatisticsViewModel.CompletedAttempts = db.QuizAttempts.Count((QuizAttempt a) => (int?)a.QuizId == id && a.IsCompleted);
            quizStatisticsViewModel.IncompleteAttempts = db.QuizAttempts.Count((QuizAttempt a) => (int?)a.QuizId == id && !a.IsCompleted);
            quizStatisticsViewModel.QuestionStatistics = GetQuestionStatistics(id.Value);
            QuizStatisticsViewModel viewModel = quizStatisticsViewModel;
            viewModel.CompletionRate = (float)viewModel.CompletedAttempts / (float)viewModel.TotalAttempts * 100f;
            return View(viewModel);
        }

        private List<QuestionStatistics> GetQuestionStatistics(int quizId)
        {
            return new List<QuestionStatistics>();
        }

       

        public ActionResult Review(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            string userId = base.User.Identity.GetUserId();
            QuizAttempt attempt = db.QuizAttempts.Include((QuizAttempt a) => a.Quiz).Include((QuizAttempt a) => a.Answers).FirstOrDefault((QuizAttempt a) => (int?)a.QuizId == id && a.StudentId == userId && a.IsCompleted);
            if (attempt == null)
            {
                return HttpNotFound();
            }
            QuizReviewViewModel viewModel = new QuizReviewViewModel
            {
                QuizTitle = attempt.Quiz.Title,
                Questions = attempt.Answers.Select((StudentAnswer a) => new QuizReviewQuestionViewModel
                {
                    QuestionNumber = a.Question.QuestionId,
                    QuestionText = a.Question.Text,
                    QuestionType = a.Question.QuestionType,
                    Options = a.Question.Options,
                    UserAnswer = a.Answer,
                    CorrectAnswer = a.Question.CorrectAnswer
                }).ToList()
            };
            return View(viewModel);
        }

        public ActionResult AvailableQuizzes()
        {
            DateTime currentTime = DateTime.Now;
            List<Quiz> availableQuizzes = (from q in db.Quizzes
                                           where q.StartTime <= currentTime && q.EndTime > currentTime
                                           orderby q.EndTime
                                           select q).ToList();
            List<Quiz> pastQuizzes = (from q in db.Quizzes
                                      where q.EndTime <= currentTime
                                      orderby q.EndTime descending
                                      select q).ToList();
            AvailableQuizzesViewModel viewModel = new AvailableQuizzesViewModel
            {
                AvailableQuizzes = availableQuizzes,
                PastQuizzes = pastQuizzes
            };
            return View(viewModel);
        }

        [Authorize]
        public ActionResult AttemptDetails(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            string userId = base.User.Identity.GetUserId();
            bool isAdminOrTutor = base.User.IsInRole("Admin") || base.User.IsInRole("Tutor");
            IQueryable<QuizAttempt> attemptQuery = db.QuizAttempts.Include((QuizAttempt qa) => qa.Quiz).Include((QuizAttempt qa) => qa.Answers.Select((StudentAnswer a) => a.Question)).Include((QuizAttempt qa) => qa.Student);
            QuizAttempt attempt = ((!isAdminOrTutor) ? attemptQuery.FirstOrDefault((QuizAttempt qa) => (int?)qa.AttemptId == id && qa.StudentId == userId) : attemptQuery.FirstOrDefault((QuizAttempt qa) => (int?)qa.AttemptId == id));
            if (attempt == null)
            {
                if (isAdminOrTutor)
                {
                    return HttpNotFound();
                }
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            if (!attempt.IsCompleted && attempt.Quiz.EndTime < DateTime.Now)
            {
                attempt.IsCompleted = true;
                attempt.EndTime = attempt.Quiz.EndTime;
                if (attempt.Score == 0)
                {
                    int totalScore = 0;
                    foreach (StudentAnswer answer in attempt.Answers)
                    {
                        if (IsAnswerCorrect(answer.Question, answer.Answer))
                        {
                            totalScore += answer.Question.Marks;
                        }
                    }
                    attempt.Score = totalScore;
                }
                db.SaveChanges();
            }
            QuizAttemptDetailsViewModel viewModel = new QuizAttemptDetailsViewModel
            {
                AttemptId = attempt.AttemptId,
                QuizId = attempt.QuizId,
                QuizTitle = attempt.Quiz.Title,
                StudentName = attempt.Student.UserName,
                StartTime = attempt.StartTime,
                EndTime = attempt.EndTime,
                Score = attempt.Score,
                TotalMarks = attempt.Quiz.Questions.Sum((Question q) => q.Marks),
                IsCompleted = attempt.IsCompleted,
                Answers = attempt.Answers.Select((StudentAnswer a) => new QuizAttemptAnswerViewModel
                {
                    AnswerId = a.AnswerId,
                    QuestionText = a.Question.Text,
                    StudentAnswer = a.Answer,
                    CorrectAnswer = a.Question.CorrectAnswer,
                    IsCorrect = IsAnswerCorrect(a.Question, a.Answer),
                    Marks = a.Question.Marks,
                    MarksObtained = a.MarksObtained
                }).ToList()
            };
            viewModel.ScorePercentage = ((viewModel.TotalMarks > 0) ? ((double)viewModel.Score / (double)viewModel.TotalMarks * 100.0) : 0.0);
            base.ViewBag.IsAdminOrTutor = isAdminOrTutor;
            base.ViewBag.CanGrade = isAdminOrTutor && !attempt.IsCompleted;
            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitAttempt(int id, List<StudentAnswer> Answers)
        {
            string userId = base.User.Identity.GetUserId();
            QuizAttempt attempt = db.QuizAttempts.Include((QuizAttempt qa) => qa.Quiz).FirstOrDefault((QuizAttempt qa) => qa.AttemptId == id && qa.StudentId == userId && !qa.IsCompleted);
            if (attempt == null)
            {
                return HttpNotFound();
            }
            attempt.IsCompleted = true;
            attempt.EndTime = DateTime.Now;
            int totalScore = 0;
            foreach (StudentAnswer answer in Answers)
            {
                Question question = db.Questions.Find(answer.QuestionId);
                if (question != null)
                {
                    bool isCorrect = IsAnswerCorrect(question, answer.Answer);
                    answer.MarksObtained = (isCorrect ? question.Marks : 0);
                    totalScore += answer.MarksObtained;
                    answer.QuizAttemptId = attempt.AttemptId;
                    db.StudentAnswers.Add(answer);
                }
            }
            attempt.Score = totalScore;
            db.SaveChanges();
            return RedirectToAction("AttemptDetails", new
            {
                id = attempt.AttemptId
            });
        }

        [Authorize(Roles = "Admin,Tutor")]
        public ActionResult Grade(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QuizAttempt quizAttempt = db.QuizAttempts.Include((QuizAttempt qa) => qa.Quiz).Include((QuizAttempt qa) => qa.Answers.Select((StudentAnswer a) => a.Question)).Include((QuizAttempt qa) => qa.Student)
                .FirstOrDefault((QuizAttempt qa) => (int?)qa.AttemptId == id);
            if (quizAttempt == null)
            {
                return HttpNotFound();
            }
            QuizAttemptDetailsViewModel viewModel = new QuizAttemptDetailsViewModel
            {
                AttemptId = quizAttempt.AttemptId,
                QuizId = quizAttempt.QuizId,
                QuizTitle = quizAttempt.Quiz.Title,
                StudentName = quizAttempt.Student.UserName,
                StartTime = quizAttempt.StartTime,
                EndTime = quizAttempt.EndTime,
                Score = quizAttempt.Score,
                TotalMarks = quizAttempt.Quiz.Questions.Sum((Question q) => q.Marks),
                IsCompleted = quizAttempt.IsCompleted,
                Answers = quizAttempt.Answers.Select((StudentAnswer a) => new QuizAttemptAnswerViewModel
                {
                    AnswerId = a.AnswerId,
                    QuestionText = a.Question.Text,
                    StudentAnswer = a.Answer,
                    CorrectAnswer = a.Question.CorrectAnswer,
                    IsCorrect = IsAnswerCorrect(a.Question, a.Answer),
                    Marks = a.Question.Marks,
                    MarksObtained = a.MarksObtained
                }).ToList()
            };
            viewModel.ScorePercentage = ((viewModel.TotalMarks > 0) ? ((double)viewModel.Score / (double)viewModel.TotalMarks * 100.0) : 0.0);
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Tutor")]
        [ValidateAntiForgeryToken]
        public ActionResult Grade(int id, QuizAttemptDetailsViewModel model)
        {
            if (model == null || id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QuizAttempt quizAttempt = db.QuizAttempts.Include((QuizAttempt qa) => qa.Answers).FirstOrDefault((QuizAttempt qa) => qa.AttemptId == model.AttemptId);
            if (quizAttempt == null)
            {
                return HttpNotFound();
            }
            if (model.Answers == null || !model.Answers.Any())
            {
                base.ModelState.AddModelError("", "No graded answers were submitted.");
                return View("AttemptDetails", model);
            }
            int totalScore = 0;
            foreach (QuizAttemptAnswerViewModel gradedAnswer in model.Answers)
            {
                StudentAnswer answer = quizAttempt.Answers.FirstOrDefault((StudentAnswer a) => a.AnswerId == gradedAnswer.AnswerId);
                if (answer != null)
                {
                    answer.MarksObtained = gradedAnswer.MarksObtained;
                    totalScore += gradedAnswer.MarksObtained;
                }
            }
            quizAttempt.Score = totalScore;
            quizAttempt.IsCompleted = true;
            db.SaveChanges();
            return RedirectToAction("AttemptDetails", new
            {
                id = quizAttempt.AttemptId
            });
        }

        [HttpPost]
        [Authorize]
        public ActionResult RateQuiz(int attemptId, int quizId, int rating, string comment)
        {
            string userId = base.User.Identity.GetUserId();
            QuizAttempt quizAttempt = db.QuizAttempts.Include((QuizAttempt qa) => qa.QuizRating).FirstOrDefault((QuizAttempt qa) => qa.AttemptId == attemptId && qa.StudentId == userId);
            if (quizAttempt == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Quiz attempt not found."
                });
            }
            if (quizAttempt.QuizRating != null)
            {
                return Json(new
                {
                    success = false,
                    message = "You have already rated this quiz attempt."
                });
            }
            QuizRating quizRating = new QuizRating
            {
                QuizId = quizId,
                StudentId = userId,
                QuizAttemptId = attemptId,
                Rating = rating,
                Comment = comment,
                DateRated = DateTime.Now
            };
            quizAttempt.QuizRating = quizRating;
            db.SaveChanges();
            return Json(new
            {
                success = true
            });
        }

        [Authorize(Roles = "Admin,Tutor")]
        public ActionResult QuizRatings(int quizId)
        {
            Quiz quiz = db.Quizzes.Include((Quiz q) => q.Module).FirstOrDefault((Quiz q) => q.QuizId == quizId);
            if (quiz == null)
            {
                return HttpNotFound();
            }
            List<QuizRating> ratings = (from qr in db.QuizRatings.Include((QuizRating qr) => qr.Student)
                                        where qr.QuizId == quizId
                                        orderby qr.DateRated descending
                                        select qr).ToList();
            QuizRatingsViewModel viewModel = new QuizRatingsViewModel
            {
                Quiz = quiz,
                Ratings = ratings
            };
            return View(viewModel);
        }

        [Authorize(Roles = "Admin,Tutor")]
        public ActionResult AllResults()
        {
            List<QuizResultSummary> allQuizzes = (from q in db.Quizzes.Include((Quiz q) => q.Module)
                                                  select new QuizResultSummary
                                                  {
                                                      Quiz = q,
                                                      TotalAttempts = db.QuizAttempts.Count((QuizAttempt a) => a.QuizId == q.QuizId),
                                                      CompletedAttempts = db.QuizAttempts.Count((QuizAttempt a) => a.QuizId == q.QuizId && a.IsCompleted),
                                                      AverageScore = db.QuizAttempts.Where((QuizAttempt a) => a.QuizId == q.QuizId && a.IsCompleted).Average((Expression<Func<QuizAttempt, double?>>)((QuizAttempt a) => (double)a.Score))
                                                  }).ToList();
            return View(allQuizzes);
        }

        [Authorize(Roles = "Admin,Tutor")]
        public ActionResult AllQuizRatings()
        {
            List<QuizRatingDisplayViewModel> allRatings = (from qr in db.QuizRatings.Include((QuizRating qr) => qr.Quiz).Include((QuizRating qr) => qr.Student).Include((QuizRating qr) => qr.QuizAttempt)
                                                           orderby qr.DateRated descending
                                                           select new QuizRatingDisplayViewModel
                                                           {
                                                               RatingId = qr.RatingId,
                                                               QuizId = qr.QuizId,
                                                               QuizTitle = qr.Quiz.Title,
                                                               StudentName = qr.Student.UserName,
                                                               Rating = qr.Rating,
                                                               Comment = qr.Comment,
                                                               DateRated = qr.DateRated,
                                                               Score = qr.QuizAttempt.Score
                                                           }).ToList();
            return View(allRatings);
        }


        public ActionResult AllQuizResults()
        {
            var quizSummaries = db.Quizzes
                .Select(q => new QuizResultSummary
                {
                    Quiz = q,
                    TotalAttempts = q.Attempts.Count(),
                    CompletedAttempts = q.Attempts.Count(a => a.IsCompleted),
                    AverageScore = q.Attempts.Where(a => a.IsCompleted).Average(a => (double?)a.Score),
                    HighestScore = q.Attempts.Where(a => a.IsCompleted).Max(a => (int?)a.Score),
                    LowestScore = q.Attempts.Where(a => a.IsCompleted).Min(a => (int?)a.Score),
                    PassRate = q.Attempts.Any(a => a.IsCompleted)
                        ? (double)q.Attempts.Count(a => a.IsCompleted && a.Score >= (q.Questions.Sum(que => que.Marks) * 0.5)) / q.Attempts.Count(a => a.IsCompleted)
                        : (double?)null
                })
                .ToList();

            return View(quizSummaries);
        }


    }

}