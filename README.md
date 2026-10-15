<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/7d52fcfa-eb4c-4dd5-bb53-03323d138aa6" /># HabiTrackers - Habit Tracking and Daily Management System

A full-stack web application designed for personal productivity, habit formation, task management, and journaling.

## Technical Architecture

* **Frontend:** HTML5, CSS3, JavaScript, ASP.NET Web Forms
* **Backend:** C#, .NET Framework
* **Database Management System:** Microsoft SQL Server

## Core Features

* **Authentication Module:** Secure user login, registration, and session management.
* **Task Management:** Create, track, update, and complete daily tasks and personal habits.
* **Journaling:** Dedicated interface for logging daily reflections and personal diary entries.
* **Productivity Tools:** Integrated study timer and customizable daily timetable scheduler.
* **Administrative Controls:** Dedicated administrative interface for user oversight and statistics monitoring.

## Repository Structure

* `Default.aspx` - System landing and entry page
* `Login.aspx` - User authentication interface
* `Dashboard.aspx` - Central user control panel
* `DailyTasks.aspx` - Daily habit and task tracking module
* `Diary.aspx` - Personal journal management interface
* `StudyTimer.aspx` - Focused productivity timer
* `Timetable.aspx` - Daily schedule organizer
* `database.sql` - Complete database schema and table initialization script

## Local Setup and Installation

1. Clone the repository to your local environment.
2. Open the solution file `HabiTrackers.sln` using Visual Studio.
3. Open Microsoft SQL Server Management Studio (SSMS), create your database, and execute the creation scripts found in `database.sql`.
4. Update the connection string parameters inside `Web.config` to point to your local SQL Server instance.
5. Press F5 in Visual Studio to build and launch the application through IIS Express.

## Project Screenshots

* **User Dashboard Interface:** `![UDashboard](images/user_dashboard.png)`
* **Habits Tracking Interface:** `![Habit](images/habits.png)`
* **Notes Tracking Interface:** `![Notes](images/notes.jpeg)`
* **Time-Table Interface:** `![TimeTable](images/timetable.jpeg)`
* **Study Timer Interface:** `![Study](images/studytimer.jpeg)`
* **Admin Dashboard Interface:** `![ADashboard](images/admin_dashboard.png)`
* **User Detail Interface:** `![Detail](images/user_detail.jpeg)`

---
Author: Arati Sutar
