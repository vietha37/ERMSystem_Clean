# Frontend Architecture

## Folder Structure

app/
dashboard/
patients/
appointments/
medical-records/
prescriptions/
login/

components/
ui/
layout/
tables/
forms/

services/
api.ts
auth.ts
patientService.ts
appointmentService.ts

hooks/
useAuth.ts
usePagination.ts

## State Strategy

* Use React hooks
* Use localStorage for token
* Axios interceptor attach token

## UI Pattern

* Card layout everywhere
* Page title + action button
* Table list + modal form
