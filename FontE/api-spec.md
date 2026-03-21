# EMR API Specification

Base URL:
http://localhost:5000/api

## Auth

POST /auth/login
POST /auth/register

## Patients

GET /patients
GET /patients/{id}
POST /patients
PUT /patients/{id}
DELETE /patients/{id}

Pagination:
GET /patients?pageNumber=1&pageSize=10

## Doctors

GET /doctors
POST /doctors

## Appointments

GET /appointments
POST /appointments

Body:
{
"patientId": "",
"doctorId": "",
"appointmentDate": "",
"status": "Pending"
}

## MedicalRecords

POST /medicalrecords

## Prescriptions

POST /prescriptions
POST /prescriptions/add-medicine

## Dashboard

GET /dashboard/stats
