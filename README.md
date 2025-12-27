🔁 Recurring Payment Scheduler

The Recurring Payment Scheduler allows users to automate recurring transactions, such as monthly rent, subscriptions, or other scheduled payments.
The system processes payments automatically, enforces business rules, and handles edge cases like insufficient funds, failed payments, and holidays.

This feature is built with scalability, reliability, and user security in mind, ensuring that recurring payments are executed accurately and auditable at all times.

✨ Key Features

Schedule automated recurring payments

Flexible frequency options: Daily, Weekly, Bi-weekly, Monthly, Quarterly, Annually

CRUD API endpoints for managing recurring payments

Background processing using Hangfire (or equivalent)

Validation for amounts and supported frequencies

Failure handling with pause after consecutive failed executions

User authorization: users can only manage their own schedules

Persistent tracking of execution history and next payment date
