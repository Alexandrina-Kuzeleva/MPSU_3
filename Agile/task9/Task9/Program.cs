using Task9.Models;
using System;

namespace Task9
{
    internal static class Program
    {
        private static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "Журнал квестов RPG";

            var inactiveLog = new QuestLog();
            var activeLog = new QuestLog();
            SeedInactiveQuests(inactiveLog);

            while (true)
            {
                WriteHeader("МЕНЮ");
                Console.WriteLine("1. Показать неактивные квесты");
                Console.WriteLine("2. Активировать квест");
                Console.WriteLine("3. Показать активные квесты (и детали)");
                Console.WriteLine("4. Удалить активный квест");
                Console.WriteLine("0. Выход");
                Console.Write("\nВыберите действие: ");

                var key = Console.ReadKey(true).KeyChar;
                Console.WriteLine();

                switch (key)
                {
                    case '1':
                        ShowQuests(inactiveLog, "Неактивные квесты", ConsoleColor.DarkYellow);
                        break;
                    case '2':
                        ActivateQuest(inactiveLog, activeLog);
                        break;
                    case '3':
                        ShowActiveQuestsWithDetails(activeLog);
                        break;
                    case '4':
                        DeleteQuest(activeLog);
                        break;
                    case '0':
                        WriteInfo("Выход...", ConsoleColor.Cyan);
                        return;
                    default:
                        WriteError("Неизвестная команда. Повторите ввод.");
                        break;
                }
            }
        }

        private static void SeedInactiveQuests(QuestLog log)
        {
            var q1 = new Quest("q001", "Охота на волков", Difficulty.Easy);
            q1.AddObjective(new Objective("kill_wolves", "Убейте 5 волков", 5));
            q1.AddObjective(new Objective("return_village", "Вернитесь к старосте"));

            var q2 = new Quest("q002", "Тайна руин", Difficulty.Hard);
            q2.AddObjective(new Objective("explore_ruins", "Исследуйте древние руины"));
            q2.AddObjective(new Objective("find_relic", "Найдите древний артефакт"));

            var q3 = new Quest("q003", "Собрать травы", Difficulty.Trivial);
            q3.AddObjective(new Objective("gather_herbs", "Соберите 10 лечебных трав", 10));

            log.Add(q1);
            log.Add(q2);
            log.Add(q3);
        }

        private static void ShowQuests(QuestLog log, string title, ConsoleColor color)
        {
            WriteHeader(title, color);

            if (log.Count == 0)
            {
                WriteInfo("Пусто.", ConsoleColor.DarkGray);
                return;
            }

            for (int i = 0; i < log.Count; i++)
            {
                var quest = log[i];
                Console.ForegroundColor = color;
                Console.WriteLine($"{i + 1}. {quest.Title} [{quest.Difficulty}]");
                Console.ResetColor();
            }
        }

        private static void ShowActiveQuestsWithDetails(QuestLog active)
        {
            if (active.Count == 0)
            {
                WriteError("Нет активных квестов.");
                return;
            }

            ShowQuests(active, "Активные квесты", ConsoleColor.Green);
            Console.Write("\nВыберите номер квеста, чтобы увидеть детали (или 0 для выхода): ");

            var key = Console.ReadKey(true).KeyChar;
            Console.WriteLine();

            if (key == '0')
                return;

            int index = key - '0';

            if (index >= 1 && index <= active.Count)
            {
                var quest = active[index - 1];
                ShowQuestDetails(quest);
            }
            else
            {
                WriteError("Неверный выбор квеста.");
            }
        }

        private static void ShowQuestDetails(Quest quest)
        {
            Console.Clear();
            WriteHeader($"Детали квеста: {quest.Title}", ConsoleColor.Cyan);
            WriteInfo($"ID: {quest.Id}", ConsoleColor.DarkGray);
            WriteInfo($"Сложность: {quest.Difficulty}", ConsoleColor.Yellow);
            Console.WriteLine();

            if (quest.Objectives.Count == 0)
            {
                WriteInfo("У этого квеста нет целей.", ConsoleColor.DarkGray);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Цели квеста:");
                Console.ResetColor();

                int i = 1;
                foreach (var obj in quest.Objectives)
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine($"  {i++}. {obj.Description} (x{obj.RequiredCount})");
                    Console.ResetColor();
                }
            }

            Console.WriteLine("\nНажмите любую клавишу, чтобы вернуться...");
            Console.ReadKey(true);
            Console.Clear();
        }

        private static void ActivateQuest(QuestLog inactive, QuestLog active)
        {
            if (inactive.Count == 0)
            {
                WriteError("Нет доступных квестов для активации.");
                return;
            }

            ShowQuests(inactive, "Неактивные квесты", ConsoleColor.Yellow);
            Console.Write("\nВыберите номер квеста (1–9) для активации: ");
            var key = Console.ReadKey(true).KeyChar;
            Console.WriteLine();

            int index = key - '0';

            if (index >= 1 && index <= inactive.Count)
            {
                var quest = inactive[index - 1];
                inactive.RemoveAt(index - 1);
                active.Add(quest);
                WriteSuccess($"Квест \"{quest.Title}\" добавлен в активные!");
            }
            else
            {
                WriteError("Неверный выбор квеста.");
            }
        }

        private static void DeleteQuest(QuestLog active)
        {
            if (active.Count == 0)
            {
                WriteError("Нет активных квестов для удаления.");
                return;
            }

            ShowQuests(active, "Активные квесты", ConsoleColor.Green);
            Console.Write("\nВыберите номер квеста (1–9) для удаления: ");
            var key = Console.ReadKey(true).KeyChar;
            Console.WriteLine();

            int index = key - '0';

            if (index >= 1 && index <= active.Count)
            {
                var title = active[index - 1].Title;
                active.RemoveAt(index - 1);
                WriteSuccess($"Квест \"{title}\" полностью удалён из журнала.");
            }
            else
            {
                WriteError("Неверный выбор квеста.");
            }
        }

        private static void WriteHeader(string text, ConsoleColor color = ConsoleColor.Cyan)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"\n{text.ToUpper()}");
            Console.ResetColor();
        }

        private static void WriteInfo(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        private static void WriteSuccess(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        private static void WriteError(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}
