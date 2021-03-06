USE [Sample123]
GO
/****** Object:  Table [dbo].[TBL_TRANSACTION_DETAILS]    Script Date: 12-10-2020 10:21:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TBL_TRANSACTION_DETAILS](
	[TransactionIdentificator] [nvarchar](50) NOT NULL,
	[Amount] [nvarchar](50) NOT NULL,
	[CurrencyCode] [nchar](3) NOT NULL,
	[TransactionDate] [datetime] NOT NULL,
	[Status] [nvarchar](15) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[PROC_GET_TRANSACTION_BY_CURRENCY]    Script Date: 12-10-2020 10:21:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[PROC_GET_TRANSACTION_BY_CURRENCY]
(
@i_CurrencyCode nvarchar(3)
)
AS

BEGIN 
select TransactionIdentificator,Amount,CurrencyCode,Status from TBL_TRANSACTION_DETAILS 
where CurrencyCode = @i_CurrencyCode
END

GO
/****** Object:  StoredProcedure [dbo].[PROC_GET_TRANSACTION_BY_DATE]    Script Date: 12-10-2020 10:21:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[PROC_GET_TRANSACTION_BY_DATE]
(
	@i_StartDate datetime,
	@i_EndDate datetime
)
AS

BEGIN 
select TransactionIdentificator,Amount,CurrencyCode,Status from TBL_TRANSACTION_DETAILS 
where TransactionDate between @i_StartDate and @i_EndDate 
END


GO
/****** Object:  StoredProcedure [dbo].[PROC_GET_TRANSACTION_BY_STATUS]    Script Date: 12-10-2020 10:21:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[PROC_GET_TRANSACTION_BY_STATUS]
(
@i_STATUS nvarchar(15)
)
AS

BEGIN 
select TransactionIdentificator,Amount,CurrencyCode,Status from TBL_TRANSACTION_DETAILS 
where Status = @i_STATUS
END

GO
