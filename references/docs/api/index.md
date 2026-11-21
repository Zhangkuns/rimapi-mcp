{% set lang = config.theme.language if config.theme.language in api_data else 'en' %}
{% set current_api = api_data.get(lang, api_data['en']) %}

{# --- Dynamically count endpoints --- #}
{% set ns = namespace(endpoint_count=0) %}
{% for controller_name, controller in current_api.controllers.items() %}
    {% for path in controller.keys() %}
        {% if path.startswith('/') %}
            {% set ns.endpoint_count = ns.endpoint_count + 1 %}
        {% endif %}
    {% endfor %}
{% endfor %}

# 📚 API Reference Overview

**Version**: {{ current_api.meta.version | default('1.8.3') }}  
**Endpoints total count**: {{ ns.endpoint_count }}

!!! warning "Warning"
    Please read our [API Conventions](../developer_guide/api_conventions.md) page to understand our `snake_case` JSON requirements and header rules before using these endpoints.

AI fork: see the [current control contracts](ai-control-contracts.md) and [native trade contract](trade-control.md). See also [outfit policy](outfit-policy.md), [idempotency](idempotency.md), the [G31 ritual instance/eligibility/start contract](ideology-ritual.md) and the [C-09 caravan formation contract](caravan-forming.md) the [C-10 caravan journey contract](caravan-journey.md) the [C-12 transport pod contract](transport-pod.md) the [C-11 caravan management contract](caravan-manage.md) the [C-39 world dialog contract](world-dialog.md) the [S-01 utility verb contract](utility-verbs.md) the [S-05 native guest policy contract](guest-policy-native.md) and [S-10 choice letter contract](letter-choices.md).
Quest player choices and Archonexus restart/core control: [contract](archonexus.md).
G-16 is delivered; game-memory idempotency has a bounded contract. Current Core,
DLC, native debug and UI documentation is indexed in [product contracts](../product-specs/index.md).
For direct map roof changes with per-cell readback, see [native roof editing](roof-grid-debug.md). For normal construction zones, see [map areas](core-areas.md).

To move an existing item stack to an exact cell in developer mode, see [whole-stack movement](item-move-debug.md).
Native UI option repairs are documented in [bill/filter/bed/facility/bookcase/blueprint controls](ui-option-repairs.md); preferences, key bindings, and native map/world display flags are in [game preferences](game-preferences.md). The [local MCP guide](../../mcp/README.md) explains live route discovery and reference snapshot updates.
See [current state](../harness/current-state.md) for validation and deployment,
and [plans](../PLANS.md) for remaining work. YAML-derived totals above do not
include all fork additions.

Welcome to the RIMAPI Reference. Choose a category below to view the available endpoints:

* [**Map & Environment**](map.md) - Weather, zones, terrain, and buildings.
* [**Pawns & AI**](pawns.md) - Pawn spawning, jobs, medical, editing, and Lord behavior.
* [**Items & Economy**](things.md) - Physical items, inventories, trading, and player designations.
* [**Game State & Mechanics**](game.md) - Save/load, random events, factions, and research.
* [**UI & Camera**](ui.md) - Viewports, windows, images, and visual overlays.
* [**System & Developer Tools**](system.md) - Cache management, API discovery, and general system diagnostics.

---

### 🤖 LLMs & Power Users
If you are using an AI coding assistant or simply prefer to search through a single, monolithic document, you can view the [**Complete API Reference (All Endpoints)**](all.md) here.
