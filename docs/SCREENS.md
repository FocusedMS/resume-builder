# Screens Overview

This document summarises the key screens of the AI‑Powered Resume Builder
frontend. Each screen corresponds to a React page under `src/pages`.

## Home

The landing page shown to guests and authenticated users. It provides a
brief overview of the product and calls to action for signing up or
logging in.

## Login

Displays a form with fields for email and password. Includes inline
validation and error messages for invalid credentials. Upon successful login
the user is redirected to their dashboard.

## Register

Allows a new user to create an account by providing email, password and
optional full name. Shows validation messages for common errors. Successful
registration routes the user to the dashboard.

## Dashboard

Lists the user’s resumes in a sortable table with columns for title and
updated timestamp. Provides actions to edit, delete and download each
resume. Admins can optionally view all resumes in the system.

## Builder

The core editing interface for creating and updating resumes. A two‑column
layout on desktop displays the form on the left and a live PDF‑like preview
on the right. The user can select a template style, edit sections with
character counters, request AI suggestions in a side drawer and download
the latest saved version.

## Admin

Accessible only to admins, this page shows a list of all users with the
ability to change roles or lock accounts. It also displays basic metrics
such as total users, total resumes and the number created in the last
24 hours. Optionally a simple chart can visualise resume creation trends.