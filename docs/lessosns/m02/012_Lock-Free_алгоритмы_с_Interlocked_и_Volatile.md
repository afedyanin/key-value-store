# Lock-Free алгоритмы с Interlocked и Volatile

## Цели занятия

- разрабатывать потокобезопасные структуры данных без использования блокировок для максимальной производительности;
- применять класс Interlocked для атомарных операций над примитивными типами;
- понимать роль Volatile и барьеров памяти для обеспечения корректности многопоточного кода.

## Краткое содержание

- атомарные операции: класс System.Threading.Interlocked;
- барьеры памяти (Memory Barriers) и их роль;
- ключевое слово volatile и класс System.Threading.Volatile;
- практические примеры: реализация потокобезопасного счетчика, lock-free стека;
- сравнение производительности lock-free и lock-based подходов.

## Результаты
- потокобезопасный счетчик на основе Interlocked и сравнительный анализ производительности с lock.

## Links

- https://learn.microsoft.com/ru-ru/dotnet/csharp/language-reference/keywords/volatile
- https://habr.com/ru/companies/wunderfund/articles/322094/
- https://learn.microsoft.com/ru-ru/archive/msdn-magazine/2012/december/csharp-the-csharp-memory-model-in-theory-and-practice
- https://dotnettutorials.net/lesson/interlocked-vs-lock-in-csharp/
- https://proglib.io/p/atomarnye-operacii-bezopasnost-potokov-i-sostoyanie-gonki-v-c-2023-08-03
- https://habr.com/ru/companies/otus/articles/868764/
- https://highload.ru/moscow/2019/abstracts/6012
- https://csharp-help.ru/2025/07/08/%D0%BF%D1%80%D0%BE%D0%B1%D0%BB%D0%B5%D0%BC%D0%B0-aba-%D0%B2-lock-free-%D0%B0%D0%BB%D0%B3%D0%BE%D1%80%D0%B8%D1%82%D0%BC%D0%B0%D1%85/
- https://csharp-help.ru/2025/07/08/%D0%B0%D1%82%D0%BE%D0%BC%D0%B0%D1%80%D0%BD%D1%8B%D0%B5-%D0%BE%D0%BF%D0%B5%D1%80%D0%B0%D1%86%D0%B8%D0%B8-%D1%81-interlocked/
- https://github.com/dotnet/runtime/blob/1d1bf92fcf43aa6981804dc53c5174445069c9e4/src/libraries/System.Private.CoreLib/src/System/Threading/Interlocked.cs
- https://learn.microsoft.com/en-us/dotnet/api/system.threading.interlocked?view=net-10.0
- https://github.com/Dzitskiy/LockFreeAlgorithms


