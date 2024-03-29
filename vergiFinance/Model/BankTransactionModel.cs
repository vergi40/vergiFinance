﻿namespace vergiFinance.Model;

internal class BankTransactionModel : IBankTransaction
{
    public DateTime RecordDate { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string Kind { get; set; }
    public string RecordType { get; set; }
    public string Recipient { get; set; }
    public string BankAccount { get; set; }
    public string Reference { get; set; }
    public string Message { get; set; }
    public string RecordId { get; set; }
}