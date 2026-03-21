# EMR Frontend Design Guidelines

## Tech Stack

* Next.js App Router
* TypeScript
* Tailwind CSS
* Axios

## Color System

Primary:

* blue-600 (main buttons)
* blue-500 (hover)
* blue-100 (background section)
* blue-50 (page background)

Status colors:

* green-500 (success)
* red-500 (error)
* yellow-500 (pending)

## Border Radius

* Cards: rounded-2xl
* Buttons: rounded-xl
* Inputs: rounded-lg
* Tables container: rounded-2xl

## Shadow

* Cards: shadow-md
* Hover: shadow-lg transition

## Layout

* Sidebar fixed left
* Header fixed top
* Content scrollable

## Spacing

* Section spacing: p-6
* Card spacing: p-4
* Gap: gap-4 or gap-6

## Table UI

* Hover highlight row
* Rounded container
* Pagination bottom-right

## Form UI

* Label above input
* Focus ring: ring-2 ring-blue-400
* Error text: text-red-500 text-sm

## UX Rules

* Show loading spinner when calling API
* Show toast success on create/update/delete
* Show confirmation modal before delete

## Dashboard Style

* Statistic cards with blue gradient
* Large number text
* Small label text

## Responsiveness

* Sidebar collapses on mobile
* Cards stack vertically
