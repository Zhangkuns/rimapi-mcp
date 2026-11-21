# 📦 Items & Economy API

This section covers spawning physical items, managing inventories, player designations, and trading with factions.

For moving an existing item stack to an exact map cell in developer mode, see [item.move](item-move-debug.md).

{% from 'api/_api_template.md' import render_controllers with context %}
{{ render_controllers([
    'ThingsController',
    'TradeController',
    'OrderController'
]) }}
