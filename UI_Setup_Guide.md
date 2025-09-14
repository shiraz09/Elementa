# מדריך הגדרת UI למשחק Elementa

## 📋 מה יצרנו:

### 1. **GameUI.cs** - סקריפט ראשי לניהול UI
### 2. **RefreshButton.cs** - כפתור רענון לוח
### 3. **MessageDisplay.cs** - הצגת הודעות על המסך

---

## 🎮 הגדרת UI ביוניטי:

### **שלב 1: יצירת Canvas**
1. לחץ ימין ב-Hierarchy → UI → Canvas
2. הגדר את ה-Canvas ל-Screen Space - Overlay
3. הוסף Canvas Scaler עם Scale With Screen Size

### **שלב 2: יצירת UI Elements**

#### **A. פאנל מהלכים וניקוד:**
1. לחץ ימין על Canvas → UI → Panel
2. שנה שם ל-"GameInfoPanel"
3. הוסף TextMeshPro components:
   - **MovesText** - להצגת מהלכים
   - **ScoreText** - להצגת ניקוד

#### **B. פאנל כוכבים:**
1. לחץ ימין על Canvas → UI → Panel
2. שנה שם ל-"StarsPanel"
3. הוסף 3 Image components לכוכבים
4. הוסף Sprites לכוכבים:
   - **StarFilled** - כוכב מלא
   - **StarEmpty** - כוכב ריק

#### **C. כפתור רענון:**
1. לחץ ימין על Canvas → UI → Button
2. שנה שם ל-"RefreshButton"
3. הוסף Image לתמונת זמן המתנה (Fill Amount)

#### **D. פאנל הודעות:**
1. לחץ ימין על Canvas → UI → Panel
2. שנה שם ל-"MessagePanel"
3. הוסף TextMeshPro להודעות
4. הוסף Image לרקע ההודעה

---

## 🔧 הגדרת הסקריפטים:

### **1. GameUI Script:**
- הוסף GameUI script לאובייקט GameInfoPanel
- הגדר את השדות:
  - **Moves Text** → MovesText
  - **Score Text** → ScoreText
  - **Star Images** → 3 כוכבים
  - **Star Filled** → Sprite כוכב מלא
  - **Star Empty** → Sprite כוכב ריק
  - **Message Display** → MessageDisplay script

### **2. RefreshButton Script:**
- הוסף RefreshButton script לכפתור הרענון
- הגדר את השדות:
  - **Refresh Button** → הכפתור עצמו
  - **Grid** → Grid object
  - **Cooldown Image** → תמונת זמן המתנה

### **3. MessageDisplay Script:**
- הוסף MessageDisplay script לפאנל ההודעות
- הגדר את השדות:
  - **Message Panel** → פאנל ההודעות
  - **Message Text** → טקסט ההודעה
  - **Message Background** → רקע ההודעה

### **4. Grid Script:**
- הוסף GameUI reference ל-Grid
- הגדר את השדה:
  - **Game UI** → GameUI script

---

## 🎨 עיצוב UI:

### **צבעים מומלצים:**
- **מהלכים:** לבן (רגיל), צהוב (מעט), אדום (מעט מאוד)
- **ניקוד:** ירוק (עלייה), לבן (רגיל)
- **כוכבים:** צהוב/זהב
- **הודעות:** צהוב (כוכבים), אדום (אזהרות), ירוק (ניקוד)

### **גדלים מומלצים:**
- **טקסט מהלכים:** 24-32px
- **טקסט ניקוד:** 28-36px
- **כוכבים:** 64x64px
- **כפתור רענון:** 100x100px

---

## 📱 מיקום UI:

### **מהלכים וניקוד:**
- מיקום: פינה שמאלית עליונה
- Anchor: Top-Left

### **כוכבים:**
- מיקום: מרכז עליון
- Anchor: Top-Center

### **כפתור רענון:**
- מיקום: פינה ימנית עליונה
- Anchor: Top-Right

### **הודעות:**
- מיקום: מרכז המסך
- Anchor: Center

---

## ✅ בדיקות:

1. **הפעל את המשחק**
2. **בדוק שהמהלכים מוצגים נכון**
3. **בדוק שהניקוד מתעדכן**
4. **בדוק שהכוכבים מוצגים**
5. **בדוק שכפתור הרענון עובד**
6. **בדוק שהודעות מוצגות**

---

## 🎯 תכונות נוספות שניתן להוסיף:

- **אנימציות חלקיקים** לכוכבים
- **צלילים** להודעות
- **אפקטים ויזואליים** לניקוד
- **מסך תוצאות** מפורט
- **סטטיסטיקות** של המשחק

---

## 🔧 פתרון בעיות:

### **UI לא מוצג:**
- בדוק שה-Canvas מוגדר נכון
- בדוק שה-TextMeshPro components מחוברים
- בדוק שה-Scripts מחוברים לאובייקטים

### **טקסט לא מתעדכן:**
- בדוק שה-GameUI מחובר ל-Grid
- בדוק שה-TextMeshPro components מוגדרים נכון
- בדוק שה-Scripts פועלים

### **כפתור לא עובד:**
- בדוק שה-Button component מחובר
- בדוק שה-RefreshButton script מחובר
- בדוק שה-Grid reference מוגדר

---

**🎮 בהצלחה עם המשחק שלך!**
