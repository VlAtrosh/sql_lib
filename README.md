# sql_lib![5289485991960244875](https://github.com/user-attachments/assets/509cb305-a06e-4b95-99f5-08f30ae3494e)


Класс Ключевые атрибуты Связи
User user_id, role ↑ Наследование → Customer
Customer address, phone → Order (1-ко-многим)
Book title, price, isbn ↔ Author (многие-ко-многим)
Author name, biography ↔ Book через BookAuthor
Order order_date, status → OrderItem (композиция)
Delivery tracking_number ← DeliveryMethod
2. Ключевые связи
Книги и авторы:

Одна книга → несколько авторов (и наоборот) через BookAuthor.


Заказы:
Customer → Order → OrderItem → Book.

Order → Delivery ← DeliveryMethod.


Экземпляры книг:
Book → BookCopy (1-ко-многим).


3. Пример логики
Клиент (Customer) создает заказ (Order).
В заказ добавляются книги (OrderItem).

Выбирается способ доставки (DeliveryMethod).

После оплаты (Payment) заказ отправляется (Delivery).

4. Что опущено
Детализация платежей (Payment).

Промежуточная таблица BookCategory.

Расширенные атрибуты (например, publisher в Book).
