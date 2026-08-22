# Tetrex
### (EN)
**Tetrex** is a 2D roguelite based on Tetris.
There are 7 stages (only 3 as of now), each one has 2 levels and a boss. After every level there is a shop, where you can buy special blocks or perms (PERManent upgrades that affect the entirety of the run)

Every track is made by *me*. There are 5 tracks per stage: 4 normal level tracks and one boss (it's impossible to encounter every one of them in a single run!)

All sprites are also made by *me*

### (RU)
**Tetrex** - 2Д роглайт, базированный на Тетрисе.
Всего есть 7 глав (пока что только 3), в каждой есть 2 обычных уровня и босс. После каждого уровня есть магазин, где можно купить особые блоки или пермы (ПЕРМанентные улучшения, влияющие на весь забег)

Вся музыка сделана *мной*. На каждую главу есть 5 треков: 4 обычных и 1 босс (невозможно услышать все за один забег!)

Все текстуры тоже сделаны *мной*

[itch.io](https://loloshara.itch.io/tetrex) page

## Versioning/Версирование
### (EN)
In alpha demo phase - X.Ya. Y goes up after bugfixes. X goes up after adding content or rebalancing existing content. Starts at 0.1a

After release - same, but without "a"

Beta branches - X.Y.Zb. X and Y are from the release the branch stems from, Z starts at 0 and goes up after every change

Subject to change

### (RU)
В фазе альфа демки - X.Ya. Y повышается после багфиксов. X повышается после добавления контента/изменений баланса. Начинается с 0.1a

После релиза - то же самое, но без "a"

В бета ветках - X.Y.Zb. X и Y берутся от версии, из которой исходит бета ветка, Z начинается с 0 и повышается после каждого изменения

Может быть изменено в будущем

# Notes for the creator/Заметки для создателя
Чтоб создать блок:
1. Скрипт
2. Префаб блока
3. Префаб превью блока (спрайт с 50% прозрачности)
4. Префаб магазинного превью блока (спрайт с 100% прозрачности)
5. Магазинный блок в PropertiesScriptableObject
6. Взвешенный блок там же

Чтоб создать блок эффект:
1. Префаб блок эффекта
2. Добавить в энум BlockEffect в DataStructures
3. Добавить префаб в PSO
4. Добавить функционал где-то в коде

Чтоб создать перм:
1. Префаб магазинного перма
2. Добавить префаб в PSO
3. Добавить в энум Perm
4. Добавить функционал где-то в коде


### TODO:
- Отображение вида перезарядки и максимального заряда пермов в магазине
- Когда выбран перм в магазине не показываются иконки свойств блоков
- Музыка для уровней и боссов с 4 по 7 главу
- Показ прогресса и уровней в магазине
- Отображение редкости и направленности блоков и пермов в магазине
- Динамическое изменение скорости шума на фоне в игре
- Механика босса: Каждые несколько ходов (или после зачистки строк?) все столбцы рандомно перемешиваются между собой
- Анимация проигрыша
- Нормальное главное меню
