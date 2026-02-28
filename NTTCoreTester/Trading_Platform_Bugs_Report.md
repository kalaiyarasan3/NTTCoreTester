# Trading Platform Bug Report  
**Detailed Issues – February 2026**  
**Compiled from client-reported and internal observations**

## 1. Order status not updating in Activity section

**Problem**  
When a BUY order for NIFTY is placed and later executed (Filled), 
the Activity section continues to show **Pending** instead of updating 
to **Filled**.

**Reproduction Steps**  
1. Client places BUY order → Status = Pending  
   Activity shows: `BUY 50 NIFTY – Pending`  
2. Order executes at exchange → Status should become Filled  
   **Expected**: `BUY 50 NIFTY – Filled`  
   **Actual**: Still shows `BUY 50 NIFTY – Pending`

**Impact**: Misleading order history, client confusion

## 2. Delay in newly placed orders appearing in Pending Orders

**Problem**  
New orders take 3–5+ seconds to appear in the Pending Orders section.

**Observed Timing**  
- 10:00:00 → Client clicks BUY  
- 10:00:02 → No order visible  
- 10:00:05 or later → Order finally appears

**Impact**: Poor user experience, clients may place duplicate orders

## 3. Incorrect Net Average Price calculation affecting MTM

**Problem**  
When multiple trades occur at different prices, the system sometimes 
calculates wrong **Net Average Price**, 
leading to inflated/deflated MTM (Mark-to-Market) values.

**Correct Example**  
- Trade 1: 50 qty @ 100  
- Trade 2: 50 qty @ 110  

Net Avg = `(50×100 + 50×110) / 100 = 10500 / 100 = 105`

MTM @ 120: `(120 − 105) × 100 = ₹1,500` profit

**Incorrect Behavior Example**  
System wrongly uses 100 as average → MTM shows ₹2,000 profit  
→ **₹500 extra profit** displayed → **Risk & Misreporting issue**

## 4. Margin remains blocked despite nil position (UCC 97118482 – NFO) 
`Completed`
**Problem**  
After square-off, position shows 0 but:  
- Order status remains **Pending**  
- Margin continues to be utilized (₹1,50,000 blocked)

**Sequence** 
1. BUY 1 lot NIFTY FUT → Margin blocked  
2. SELL 1 lot (square off) → Position = 0  
   **Expected**: Margin released, no pending order  
   **Actual**: Margin still blocked, order shown as Pending

## 5. Insufficient margin validation on opposite-side orders

**Problem**  
Client with existing short position (–2 lots) places buy order for 3 lots 
→ system allows full 3 lots 
instead of restricting excess quantity.

**Expected**  
- Allow 2 lots (square off existing short)  
- Validate margin only for the additional 1 lot  
- Reject excess if margin insufficient

**Actual**  
Full 3 lots accepted → extra 1 lot treated as new long position 
→ unnecessary extra margin blocked

## 6. Additional position allowed despite insufficient margin (Natural Gas)

**Problem**  
Client with open position could add more lots even when available margin 
was clearly insufficient.

**Example**  
- Existing: 1 lot, margin used ₹1,20,000  
- Available balance: ₹20,000  
- Margin per lot: ₹1,20,000  

**Expected**: Reject new BUY 1 lot  
**Actual**: Order accepted → position = 2 lots, severely under-margined

**Severity**: Clear **RMS (Risk Management System)** failure

## 7. Position taken in NGMini without sufficient balance (UCC 98403013)

**Problem**  
Fresh position opened in Natural Gas Mini despite 
available balance < required margin.

**Example**  
- Required per lot: ₹40,000  
- Available: ₹15,000  

**Expected**: Order rejected  
**Actual**: Position opened → account under-margined

## 8. Duplicate / excess unpledge processed (UCC 98073433 – MTF)

**Problem**  
Client pledged 20 shares of AMBUJACEM → system processed unpledge 
requests totaling **34 shares**.

**Sequence**  
- Pledged: 20 qty  
- Unpledge 20 → successful  
- Unpledge 14 → **also accepted** (should have been rejected)

**Impact**: Over-unpledging, potential collateral mismatch

## 9. Modification of rejected order allowed (CRUDEOILM 4700 PE)

**Problem**  
Rejected limit order was allowed to be modified to Market → executed 
successfully, 
but modification confirmation arrived **after** trade confirmation.

**Wrong Sequence Observed**  
1. Order Rejected  
2. Modified to Market  
3. Trade Executed  
4. Modification Confirmation received

**Expected**: Rejected orders should be in terminal state — modification 
not allowed

## 10. Cannot modify pending limit order to market

**Problem**  
Pending (open) limit order cannot be modified to Market — error message shown.

**Expected**  
Pending order → modify to Market → should be sent to exchange and likely 
execute immediately

**Actual**: Error thrown ("Modification not allowed" / similar)

## 11. Cannot transfer margin from MTF to Non-MTF (Client NR000356)

**Problem**  
Available MTF margin cannot be transferred to normal (Non-MTF) trading account.

**Expected**  
Transfer allowed (subject to policy) → funds move between buckets

**Actual**: Transfer blocked / not permitted

## 12. Error when cancelling AMO orders

**Problem**  
After-market orders (AMO) cannot be cancelled before 
market opens — cancellation fails with error.

**Expected**  
- Cancel AMO before market open → status → Cancelled  
- Margin (if any) released

**Actual**: Error during cancellation attempt

---

**Summary of Critical Items (High Severity)**  
- RMS / Margin validation failures (#4, #5, #6, #7)  
- Incorrect average price & MTM (#3)  
- Stuck margin after square-off (#4)  
- Over-unpledging (#8)  
- Wrong order state handling (#9, #10)

**Recommendation**  
Prioritize RMS logic, order state machine, margin release on square-off, 
and average price calculation fixes.