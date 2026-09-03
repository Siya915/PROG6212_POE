-- RaceDay Database
-- Beginner-friendly SQL Server script
-- Run this script once on a clean SQL Server instance.

CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO

-- 1. Users
CREATE TABLE Users
(
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Phone VARCHAR(20),
    Role VARCHAR(20) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),

    CHECK (Role = 'Participant' OR Role = 'Organizer')
);
GO

-- 2. Events
CREATE TABLE Events
(
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    OrganizerID INT NOT NULL,
    EventName VARCHAR(100) NOT NULL,
    EventDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    Venue VARCHAR(150) NOT NULL,
    Description VARCHAR(500),
    Capacity INT NOT NULL,
    Status VARCHAR(20) DEFAULT 'Open',
    CreatedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (OrganizerID) REFERENCES Users(UserID),
    CHECK (Capacity > 0),
    CHECK (Status = 'Open' OR Status = 'Closed' OR Status = 'Completed')
);
GO

-- 3. Categories
CREATE TABLE Categories
(
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName VARCHAR(80) NOT NULL UNIQUE,
    Description VARCHAR(250)
);
GO

-- 4. EventCategories
-- This table connects Events and Categories.
CREATE TABLE EventCategories
(
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,

    PRIMARY KEY (EventID, CategoryID),
    FOREIGN KEY (EventID) REFERENCES Events(EventID),
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
);
GO

-- 5. Enrolments
CREATE TABLE Enrolments
(
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    ParticipantID INT NOT NULL,
    EnrolmentDate DATETIME DEFAULT GETDATE(),
    Status VARCHAR(20) DEFAULT 'Confirmed',

    FOREIGN KEY (EventID) REFERENCES Events(EventID),
    FOREIGN KEY (ParticipantID) REFERENCES Users(UserID),
    UNIQUE (EventID, ParticipantID),
    CHECK (Status = 'Confirmed' OR Status = 'Cancelled')
);
GO

-- 6. Results
CREATE TABLE Results
(
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL,
    FinishTime TIME,
    Position INT,
    Points INT,
    RecordedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (EnrolmentID) REFERENCES Enrolments(EnrolmentID),
    UNIQUE (EnrolmentID),
    CHECK (Position IS NULL OR Position > 0),
    CHECK (Points IS NULL OR Points >= 0)
);
GO

-- Sample Users
-- The password values below are example hashes/text for database seed data.
INSERT INTO Users
    (FirstName, LastName, Email, PasswordHash, Phone, Role)
VALUES
    ('Amanda', 'Mokoena', 'amanda@raceday.co.za', 'hash_amanda_123', '0712345678', 'Organizer'),
    ('Brian', 'Nkosi', 'brian@raceday.co.za', 'hash_brian_123', '0723456789', 'Organizer'),
    ('Siyabonga', 'Madiba', 'siya@example.com', 'hash_siya_123', '0734567890', 'Participant'),
    ('Lerato', 'Khumalo', 'lerato@example.com', 'hash_lerato_123', '0745678901', 'Participant');
GO

-- Sample Categories
INSERT INTO Categories (CategoryName, Description)
VALUES
    ('5 km Fun Run', 'Short community running event'),
    ('10 km Road Race', 'Competitive 10 km race'),
    ('21 km Half Marathon', 'Long distance half marathon');
GO

-- Sample Events
INSERT INTO Events
    (OrganizerID, EventName, EventDate, StartTime, Venue, Description, Capacity, Status)
VALUES
    (1, 'Kgapane Community Run', '2026-10-10', '07:00', 'Kgapane Stadium',
     'Community fun run', 500, 'Open'),

    (2, 'Polokwane Spring 10K', '2026-10-24', '06:30', 'Peter Mokaba Stadium',
     '10 km road race', 750, 'Open'),

    (1, 'Limpopo Half Marathon', '2026-11-15', '06:00', 'Polokwane Civic Centre',
     '21 km half marathon', 1000, 'Open');
GO

-- Connect events to categories
INSERT INTO EventCategories (EventID, CategoryID)
VALUES
    (1, 1),
    (2, 1),
    (2, 2),
    (3, 2),
    (3, 3);
GO

-- Sample Enrolments
INSERT INTO Enrolments (EventID, ParticipantID, Status)
VALUES
    (1, 3, 'Confirmed'),
    (2, 3, 'Confirmed'),
    (2, 4, 'Confirmed'),
    (3, 4, 'Confirmed');
GO

-- Sample Results
INSERT INTO Results (EnrolmentID, FinishTime, Position, Points)
VALUES
    (1, '00:28:42', 4, 80),
    (2, '00:49:31', 8, 70);
GO

-- Check that the data was added
SELECT * FROM Users;
SELECT * FROM Categories;
SELECT * FROM Events;
SELECT * FROM EventCategories;
SELECT * FROM Enrolments;
SELECT * FROM Results;
GO
