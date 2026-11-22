# Compact Upstream YAML Index

These examples are not the full fork catalog. Use the [static route index](routes.md)
for declared method/path pairs and the [API guide](index.md) for contracts and live discovery.

{# FORCE ENGLISH: Bypasses the i18n overwrite loop and fixes Windows encoding bugs #}
{% set current_api = api_data['en'] %}

{% for controller_name, controller in current_api.controllers.items() %}
{% for path, data in controller.items() %}
{% if path.startswith('/') %}
{{ data.method | default('GET') | upper }} {{ path }} - {{ data.desc | striptags | replace('\n', ' ') | truncate(150) }}
{% endif %}
{% endfor %}
{% endfor %}
