# Upstream YAML Reference

This page renders all entries in the upstream YAML reference, not all endpoints
in this fork. See the [API guide](index.md), [static route index](routes.md), and
[live discovery](../capabilities/capabilities.md) for current coverage and usage.

{% from 'api/_api_template.md' import render_controllers with context %}
{% set lang = config.theme.language if config.theme.language in api_data else 'en' %}
{{ render_controllers(api_data.get(lang, api_data['en']).controllers.keys()) }}
