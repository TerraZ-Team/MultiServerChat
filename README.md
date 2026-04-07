# MultiServerChat
Изначально создан Zack Piispanen. Сейчас поддерживается и обновляется Ryozuki и TerraZ Team.

## Стек
- `TZ.TShockAPI` `6.1.0`
- `.NET 9`

## Назначение
Плагин пересылает сообщения чата, входы и выходы игроков между несколькими TShock-серверами.

## Команды
Команда | Право | Описание
--- | --- | ---
`/msc_reload` | `msc.reload` | Перезагружает конфиг `multiserverchat.json`.

## Права
Право | Описание
--- | ---
`msc.reload` | Разрешает перезагрузку конфига через `/msc_reload` и стандартный reload TShock.
`msc.canchat` | Нужно REST-токену, через который другие серверы отправляют сообщения в этот сервер.

## Конфиг
Файл плагина создаётся в `tshock/multiserverchat.json`.

Пример:
```json
{
  "RestURLs": [
    "http://server-1:7878",
    "http://server-2:7878"
  ],
  "Token": "TOKENYOULIKE",
  "ChatFormat": "[{0}] {1}",
  "JoinFormat": "[{0}] {1} has joined.",
  "LeaveFormat": "[{0}] {1} has left.",
  "SendChat": true,
  "SendJoinLeave": true,
  "DisplayChat": true,
  "DisplayJoinLeave": true
}
```

Поля:
- `RestURLs` — список адресов других серверов TShock.
- `Token` — REST-токен TShock, который используется для отправки сообщений на другие серверы.
- `ChatFormat` — формат пересылаемого чата. `{0}` — имя сервера, `{1}` — текст сообщения.
- `JoinFormat` — формат сообщения о входе игрока.
- `LeaveFormat` — формат сообщения о выходе игрока.
- `SendChat` — отправлять ли локальный чат на другие серверы.
- `SendJoinLeave` — отправлять ли события входа/выхода на другие серверы.
- `DisplayChat` — показывать ли входящий удалённый чат на этом сервере.
- `DisplayJoinLeave` — показывать ли входящие удалённые события входа/выхода на этом сервере.

## Настройка TShock REST
Плагин принимает входящие сообщения через REST-роуты:
- `/msc`
- `/jl`

Оба route защищены правом `msc.canchat`, поэтому REST-токен должен быть связан с группой, у которой есть это право.

Токен можно добавить в `tshock/config.json`, в секцию `ApplicationRestTokens`:
```json
"ApplicationRestTokens": {
  "TOKENYOULIKE": {
    "Username": "Ryozuki",
    "UserGroupName": "superadmin"
  }
}
```

После этого у соответствующей группы должно быть право `msc.canchat`.

## Транспорт
Текущая отправка идёт через `POST` на `/jl?token=...` с JSON-телом.

Для совместимости входящий обработчик также умеет читать старый формат, где сообщение передаётся через query-параметр `message`.
