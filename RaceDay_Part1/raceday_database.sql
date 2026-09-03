/*
    RaceDay Database Script
    Platform: Microsoft SQL Server / SSMS
    Purpose: Creates and seeds the complete RaceDay relational database.

    Entities:
      1. Users
      2. Events
      3. Categories
      4. EventCategories
      5. Enrolments
      6. Results

    Roles:
      Participant - registers, logs in, manages profile, views events/categories,
                    enrols in events and views own results.
      Organizer   - creates/updates/deletes events and categories, manages event
                    enrolments and records results.
*/

IF DB_ID(N'RaceDayDB') IS NULL
BEGIN
    CREATE DATABASE RaceDayDB;
END;
GO

USE RaceDayDB;
GO

/* Drop existing tables in dependency order so the script can be re-run. */
IF OBJECT_ID(N'dbo.Results', N'U') IS NOT NULL DROP TABLE dbo.Results;
IF OBJECT_ID(N'dbo.Enrolments', N'U') IS NOT NULL DROP TABLE dbo.Enrolments;
IF OBJECT_ID(N'dbo.EventCategories', N'U') IS NOT NULL DROP TABLE dbo.EventCategories;
IF OBJECT_ID(N'dbo.Categories', N'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID(N'dbo.Events', N'U') IS NOT NULL DROP TABLE dbo.Events;
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL DROP TABLE dbo.Users;
GO

/* ============================================================
   1. USERS
   ============================================================ */
CREATE TABLE dbo.Users
(
    UserID       INT IDENTITY(1,1) NOT NULL,
    FirstName    NVARCHAR(50) NOT NULL,
    LastName     NVARCHAR(50) NOT NULL,
    Email        NVARCHAR(120) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Phone        NVARCHAR(20) NULL,
    Role         NVARCHAR(20) NOT NULL
        CONSTRAINT CK_Users_Role CHECK (Role IN (N'Participant', N'Organizer')),
    CreatedAt    DATETIME2 NOT NULL
        CONSTRAINT DF_Users_CreatedAt DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Users PRIMARY KEY (UserID),
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);
GO

/* ============================================================
   2. EVENTS
   ============================================================ */
CREATE TABLE dbo.Events
(
    EventID       INT IDENTITY(1,1) NOT NULL,
    OrganizerID   INT NOT NULL,
    EventName     NVARCHAR(100) NOT NULL,
    EventDate     DATE NOT NULL,
    StartTime     TIME NOT NULL,
    Venue         NVARCHAR(150) NOT NULL,
    Description   NVARCHAR(500) NULL,
    Capacity      INT NOT NULL,
    Status        NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Events_Status DEFAULT N'Open',
    CreatedAt     DATETIME2 NOT NULL
        CONSTRAINT DF_Events_CreatedAt DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Events PRIMARY KEY (EventID),
    CONSTRAINT FK_Events_Organizer
        FOREIGN KEY (OrganizerID) REFERENCES dbo.Users(UserID),
    CONSTRAINT CK_Events_Capacity CHECK (Capacity > 0),
    CONSTRAINT CK_Events_Status CHECK (Status IN (N'Open', N'Closed', N'Completed'))
);
GO

/* ============================================================
   3. CATEGORIES
   ============================================================ */
CREATE TABLE dbo.Categories
(
    CategoryID   INT IDENTITY(1,1) NOT NULL,
    CategoryName NVARCHAR(80) NOT NULL,
    Description  NVARCHAR(250) NULL,

    CONSTRAINT PK_Categories PRIMARY KEY (CategoryID),
    CONSTRAINT UQ_Categories_Name UNIQUE (CategoryName)
);
GO

/* ============================================================
   4. EVENT-CATEGORY JUNCTION
      Resolves the many-to-many relationship between Events
      and Categories.
   ============================================================ */
CREATE TABLE dbo.EventCategories
(
    EventID    INT NOT NULL,
    CategoryID INT NOT NULL,

    CONSTRAINT PK_EventCategories PRIMARY KEY (EventID, CategoryID),
    CONSTRAINT FK_EventCategories_Event
        FOREIGN KEY (EventID) REFERENCES dbo.Events(EventID)
        ON DELETE CASCADE,
    CONSTRAINT FK_EventCategories_Category
        FOREIGN KEY (CategoryID) REFERENCES dbo.Categories(CategoryID)
        ON DELETE CASCADE
);
GO

/* ============================================================
   5. ENROLMENTS
   ============================================================ */
CREATE TABLE dbo.Enrolments
(
    EnrolmentID   INT IDENTITY(1,1) NOT NULL,
    EventID       INT NOT NULL,
    ParticipantID INT NOT NULL,
    EnrolmentDate DATETIME2 NOT NULL
        CONSTRAINT DF_Enrolments_Date DEFAULT SYSDATETIME(),
    Status        NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Enrolments_Status DEFAULT N'Confirmed',

    CONSTRAINT PK_Enrolments PRIMARY KEY (EnrolmentID),
    CONSTRAINT FK_Enrolments_Event
        FOREIGN KEY (EventID) REFERENCES dbo.Events(EventID)
        ON DELETE CASCADE,
    CONSTRAINT FK_Enrolments_Participant
        FOREIGN KEY (ParticipantID) REFERENCES dbo.Users(UserID),
    CONSTRAINT UQ_Enrolments_EventParticipant UNIQUE (EventID, ParticipantID),
    CONSTRAINT CK_Enrolments_Status CHECK
        (Status IN (N'Confirmed', N'Cancelled'))
);
GO

/* ============================================================
   6. RESULTS
   One result belongs to one enrolment.
   ============================================================ */
CREATE TABLE dbo.Results
(
    ResultID     INT IDENTITY(1,1) NOT NULL,
    EnrolmentID  INT NOT NULL,
    FinishTime   TIME NULL,
    Position     INT NULL,
    Points       INT NULL,
    RecordedAt   DATETIME2 NOT NULL
        CONSTRAINT DF_Results_RecordedAt DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Results PRIMARY KEY (ResultID),
    CONSTRAINT FK_Results_Enrolment
        FOREIGN KEY (EnrolmentID) REFERENCES dbo.Enrolments(EnrolmentID)
        ON DELETE CASCADE,
    CONSTRAINT UQ_Results_Enrolment UNIQUE (EnrolmentID),
    CONSTRAINT CK_Results_Position CHECK (Position IS NULL OR Position > 0),
    CONSTRAINT CK_Results_Points CHECK (Points IS NULL OR Points >= 0)
);
GO

/* ============================================================
   SEED DATA
   At least: 2 organisers, 2 participants, 3 events,
   categories for every event, and sample enrolments/results.
   ============================================================ */

INSERT INTO dbo.Users
    (FirstName, LastName, Email, PasswordHash, Phone, Role)
VALUES
    (N'Amanda', N'Mokoena', N'amanda@raceday.co.za',
     CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'Amanda@123'), 2),
     N'0712345678', N'Organizer'),
    (N'Brian', N'Nkosi', N'brian@raceday.co.za',
     CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'Brian@123'), 2),
     N'0723456789', N'Organizer'),
    (N'Siyabonga', N'Madiba', N'siya@example.com',
     CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'Siya@123'), 2),
     N'0734567890', N'Participant'),
    (N'Lerato', N'Khumalo', N'lerato@example.com',
     CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'Lerato@123'), 2),
     N'0745678901', N'Participant');
GO

INSERT INTO dbo.Categories (CategoryName, Description)
VALUES
    (N'5 km Fun Run', N'Accessible short-distance community running event.'),
    (N'10 km Road Race', N'Competitive road race for intermediate runners.'),
    (N'21 km Half Marathon', N'Long-distance half marathon event.');
GO

INSERT INTO dbo.Events
    (OrganizerID, EventName, EventDate, StartTime, Venue, Description, Capacity, Status)
VALUES
    (1, N'Kgapane Community Run', '2026-10-10', '07:00',
     N'Kgapane Stadium', N'Community fun run promoting healthy living.', 500, N'Open'),
    (2, N'Polokwane Spring 10K', '2026-10-24', '06:30',
     N'Peter Mokaba Stadium', N'Organised 10 km road race for local athletes.', 750, N'Open'),
    (1, N'Limpopo Half Marathon', '2026-11-15', '06:00',
     N'Polokwane Civic Centre', N'Official 21 km half marathon.', 1000, N'Open');
GO

INSERT INTO dbo.EventCategories (EventID, CategoryID)
VALUES
    (1, 1),
    (2, 2),
    (2, 1),
    (3, 3),
    (3, 2);
GO

INSERT INTO dbo.Enrolments (EventID, ParticipantID, EnrolmentDate, Status)
VALUES
    (1, 3, '2026-09-01T09:15:00', N'Confirmed'),
    (2, 3, '2026-09-01T09:30:00', N'Confirmed'),
    (2, 4, '2026-09-02T10:00:00', N'Confirmed'),
    (3, 4, '2026-09-02T10:15:00', N'Confirmed');
GO

INSERT INTO dbo.Results (EnrolmentID, FinishTime, Position, Points)
VALUES
    (1, '00:28:42', 4, 80),
    (2, '00:49:31', 8, 70);
GO

/* ============================================================
   VERIFICATION QUERIES
   ============================================================ */
SELECT * FROM dbo.Users;
SELECT * FROM dbo.Categories;
SELECT * FROM dbo.Events;
SELECT * FROM dbo.EventCategories;
SELECT * FROM dbo.Enrolments;
SELECT * FROM dbo.Results;
GO
