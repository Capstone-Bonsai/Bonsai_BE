# Thanh Son Garden Project

PLEASE READ THIS FILE CAREFULLY!

## EF migration code first

To apply the latest migrations to your physical database, run this command(run the command from <strong>Bonsai_BE solution folder</strong>)

```
We do not have it right now.
```

## Branch convention

- For every new function, you must create a new branch, then invite other members to review your code in Git Hub, when that branch merged successfully, you can delete it and continue to create a new branch based on main for other functions.
- When you create a new branch, it must be based on the **main branch** and follow this naming convention:
  **"YourName_FunctionName"**

## Commit naming convention

When you add a new commit, it must follow this naming convention:
**"[YourName][Action description]"**

## Pull request convention

You must create a pull request before your code is merged into main. The pull request must be followed this naming convention:
**"[YourName][YourFunction]"**

## EF migration

0. install global tool to make migration(do only 1 time & your machine is good to go for the next)

```
dotnet tool install --global dotnet-ef
```

1. create migrations & the dbcontext snapshot will rendered.
   Open CLI at apis folder & run command
   -s is startup project(create dbcontext instance at design time)
   -p is migrations assembly project

```
dotnet ef migrations add NewMigration -s WebAPI -p Infrastructures
```

2. apply the change

```
dotnet ef database update -s WebAPI -p Infrastructures
```

## Spring2024SE079_Bonsai Capstone Project

1. Do Thanh Bo
2. Nguyen Thi Tra My
3. Phan Truong Minh Dang
4. Hoang Dinh Thai
