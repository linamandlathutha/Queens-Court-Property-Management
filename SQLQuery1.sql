UPDATE dbo.Histories
SET PaymentStatus = 0
WHERE  Id=6; -- Only change if the current status is Paid

update dbo.Histories
set MonthlyRent=4500
where Id=4;


update dbo.Histories
set DueDate='2024/12/04'
where Id=11; -- Make sure to set Id to a record that exists and background task will check every 3 minutes