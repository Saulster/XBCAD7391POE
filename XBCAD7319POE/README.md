# Feenix Survey Completion Confirmation Tool

## Mission
To ensure that young people have access to education and are equipped to thrive in life. This is done through connecting young people in need to donor communities that care and critical wrap-around support services.

## Vision
A society in which all young people are able to fulfill their potential and positively contribute toward a healthier society regardless of wealth.

---

## Overview
The **Feenix Survey Completion Confirmation Tool** streamlines the process of verifying survey completions for students in the Feenix program. It automates the moderation process and enhances efficiency by leveraging the SurveyMonkey API, AWS services, and a user-friendly interface.

---

## Project Objectives
The primary goal is to develop a tool for Feenix admins to:
1. **Verify survey completion for individual students** using their unique identifiers.
2. **Process bulk verifications** through CSV uploads.
3. **Store and manage survey data** securely on AWS services.

---

## Features
### 1. User Authentication and Access Control
- Secure login system for Feenix admins.
- Access control ensures only authorized users can access the tool.

### 2. Single Student Survey Check
- Input a student's unique identifier.
- Fetch and display the survey completion status using the SurveyMonkey API.

### 3. Bulk Student Survey Check
- Upload a CSV containing multiple student IDs.
- Save the uploaded CSV to Amazon S3.
- Validate and parse the file to query SurveyMonkey API for survey statuses.
- Store responses in an Amazon RDS Postgres database.
- Provide feedback on completions, errors, or mismatched formats.

### 4. Data Storage and Management
- Store uploaded CSVs securely in Amazon S3.
- Save survey responses in an Amazon RDS Postgres database.
- Bulk check history: List past uploads and allow downloading of processed files.

### 5. Admin Dashboard
- View survey completion statuses.
- Recheck previously checked StudentIDs
- Search for a StudentID

### 6. User Interface
- Built with **Next.js** for a modern and responsive design.
- Easy navigation for single or bulk checks.
- Clear and comprehensible display of results.

---

## Usage
Once the application is set up and running:

### Log In:
Use your admin credentials to access the system.

### Perform a Single Check:
Navigate to the Single Check page.
Enter a student's unique ID to fetch their survey completion status.

### Perform a Bulk Check:
Navigate to the Bulk Check page.
Upload a CSV file containing student IDs.
View the completion statuses of all students listed in the file.

### View History:
Access the Bulk Check History to download previous CSV files and review survey statuses.

---

## Technical Requirements
### Technology Stack
#### **Frontend**
- Framework: [Next.js](https://nextjs.org/)
- Authentication: [JWT](https://jwt.io/)

#### **Backend**
- Language: C#
- Framework: [ASP.NET Core](https://dotnet.microsoft.com/)
- API Integration: [SurveyMonkey API](https://api.surveymonkey.com/v3/docs#overview)

#### **Database and Storage**
- Amazon RDS Postgres for survey data.
- Amazon S3 for uploaded files.

#### **Hosting**
- Hosted on [AWS](https://aws.amazon.com/).

---

## Setup and Installation

### Prerequisites
Before you begin, ensure you have the following:
- [Next.js](https://nextjs.org/) installed for the frontend.
- [.NET SDK](https://dotnet.microsoft.com/) installed for the backend.
- An AWS account with S3 and RDS set up.
- SurveyMonkey API credentials.

### Installation Steps
1. **Clone the repository**:
   ```bash
   git clone https://github.com/Allana-Morris/XBCAD7319POE.git
   cd XBCAD7319POE

2. **Install dependencies for the frontend**:
   ```bash
   cd frontend
   npm install

3. **Build and run the backend**:
   ```bash
   cd WebApplication1
   dotnet build
   dotnet run

4. **Set up environment variables**:
- Add AWS credentials and SurveyMonkey API keys to the .env files in backend directory.

5. **Deploy to AWS**:
- Configure the application to use Amazon S3 for file storage.
- Set up an Amazon RDS Postgres database to store survey completion data.

---
