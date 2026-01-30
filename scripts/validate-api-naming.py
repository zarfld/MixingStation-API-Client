#!/usr/bin/env python3
"""
API Naming Policy Validator

Enforces "Public Interface Mirrors REST" policy (ADR-002).
Validates that public methods in MixingStation.Client.dll match REST endpoint naming rules.

Usage:
    python validate-api-naming.py --openapi openapi.json --assembly MixingStation.Client.dll

Exit codes:
    0 - All methods comply with naming policy
    1 - Naming violations found
    2 - Script error (missing files, parse errors)

Standards: ISO/IEC/IEEE 42010:2011 (Architecture enforcement)
"""

import argparse
import json
import re
import sys
from pathlib import Path
from typing import List, Dict, Tuple, Optional

# Color codes for terminal output
class Colors:
    GREEN = '\033[92m'
    RED = '\033[91m'
    YELLOW = '\033[93m'
    BLUE = '\033[94m'
    END = '\033[0m'

def parse_openapi(openapi_path: Path) -> Dict[str, List[Dict]]:
    """
    Parse OpenAPI spec and extract expected method names.
    
    Returns:
        Dict mapping namespace (App, Console) to list of expected methods
    """
    try:
        with open(openapi_path, 'r', encoding='utf-8') as f:
            spec = json.load(f)
    except FileNotFoundError:
        print(f"{Colors.RED}❌ OpenAPI file not found: {openapi_path}{Colors.END}")
        sys.exit(2)
    except json.JSONDecodeError as e:
        print(f"{Colors.RED}❌ Invalid JSON in OpenAPI file: {e}{Colors.END}")
        sys.exit(2)
    
    expected_methods = {
        'App': [],
        'Console': []
    }
    
    paths = spec.get('paths', {})
    
    for path, methods in paths.items():
        # Determine namespace from first path segment
        parts = path.strip('/').split('/')
        if not parts:
            continue
        
        group = parts[0].capitalize()  # 'app' → 'App', 'console' → 'Console'
        
        if group not in expected_methods:
            print(f"{Colors.YELLOW}⚠️  Unknown group: {group} (from path {path}){Colors.END}")
            continue
        
        for http_method, operation in methods.items():
            if http_method.lower() not in ['get', 'post', 'put', 'delete', 'patch']:
                continue
            
            # Build expected method name
            method_name = derive_method_name(path, http_method)
            
            expected_methods[group].append({
                'name': method_name,
                'path': path,
                'http_method': http_method.upper(),
                'operation_id': operation.get('operationId'),
                'group': group
            })
    
    return expected_methods

def derive_method_name(path: str, http_method: str) -> str:
    """
    Derive C# method name from REST path using ADR-002 naming rules.
    
    Rules:
    1. Verb = HTTP method (Get, Post, Put, Delete)
    2. PathSegments = Path parts after group, PascalCase
    3. Suffix = Async
    
    Examples:
    - GET /app/mixers/current → GetMixersCurrentAsync
    - POST /app/connect → ConnectAsync (exception: single-verb endpoint)
    - GET /console/information → GetInformationAsync
    - GET /console/data/get/{path} → GetDataGetAsync
    """
    parts = path.strip('/').split('/')
    
    if len(parts) < 2:
        return f"{http_method.capitalize()}Async"
    
    # Remove first segment (group: app, console, etc.)
    path_segments = parts[1:]
    
    # Remove path parameters like {path}, {id}
    path_segments = [seg for seg in path_segments if not seg.startswith('{')]
    
    # Convert to PascalCase
    pascal_parts = [seg.capitalize() for seg in path_segments]
    
    # Check for single-verb exception (connect, disconnect, subscribe)
    single_verb_endpoints = ['connect', 'disconnect', 'subscribe', 'unsubscribe']
    if len(pascal_parts) == 1 and pascal_parts[0].lower() in single_verb_endpoints:
        # Exception: /app/connect → ConnectAsync (not PostConnectAsync)
        return f"{pascal_parts[0]}Async"
    
    # Standard rule: Verb + PathSegments + Async
    verb = http_method.capitalize()
    path_part = ''.join(pascal_parts)
    
    return f"{verb}{path_part}Async"

def extract_actual_methods_from_assembly(assembly_path: Path) -> Dict[str, List[str]]:
    """
    Extract actual public method names from compiled .NET assembly.
    
    Uses .NET reflection via PowerShell (since we're on Windows).
    
    Returns:
        Dict mapping class name (AppClient, ConsoleClient) to list of method names
    """
    if not assembly_path.exists():
        print(f"{Colors.RED}❌ Assembly not found: {assembly_path}{Colors.END}")
        print(f"{Colors.YELLOW}💡 Hint: Build the project first with 'dotnet build'{Colors.END}")
        sys.exit(2)
    
    # PowerShell script to reflect over assembly
    ps_script = f"""
    $assembly = [System.Reflection.Assembly]::LoadFrom("{assembly_path.absolute()}")
    $types = $assembly.GetExportedTypes()
    
    $result = @{{}}
    
    foreach ($type in $types) {{
        if ($type.Name -like "*Client" -and -not $type.IsInterface) {{
            $methods = $type.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
            $methodNames = $methods | Where-Object {{ -not $_.IsSpecialName }} | Select-Object -ExpandProperty Name
            $result[$type.Name] = $methodNames
        }}
    }}
    
    $result | ConvertTo-Json
    """
    
    try:
        import subprocess
        result = subprocess.run(
            ['powershell', '-Command', ps_script],
            capture_output=True,
            text=True,
            check=True
        )
        
        # Parse JSON output
        actual_methods = json.loads(result.stdout)
        
        # Convert PowerShell arrays to Python lists
        for class_name in actual_methods:
            if isinstance(actual_methods[class_name], dict):
                # PowerShell returned object instead of array (single item)
                actual_methods[class_name] = [actual_methods[class_name]]
        
        return actual_methods
    
    except subprocess.CalledProcessError as e:
        print(f"{Colors.RED}❌ Failed to reflect over assembly:{Colors.END}")
        print(e.stderr)
        sys.exit(2)
    except json.JSONDecodeError as e:
        print(f"{Colors.RED}❌ Failed to parse PowerShell output:{Colors.END}")
        print(result.stdout)
        sys.exit(2)

def validate_naming(expected: Dict[str, List[Dict]], actual: Dict[str, List[str]]) -> Tuple[int, int]:
    """
    Validate actual methods against expected methods.
    
    Returns:
        Tuple of (violations_count, total_checks)
    """
    violations = 0
    total_checks = 0
    
    print(f"\n{Colors.BLUE}═══ API Naming Policy Validation (ADR-002) ═══{Colors.END}\n")
    
    for group, expected_methods in expected.items():
        class_name = f"{group}Client"
        actual_methods = actual.get(class_name, [])
        
        print(f"\n{Colors.BLUE}── {class_name} ──{Colors.END}")
        
        for expected_method in expected_methods:
            total_checks += 1
            method_name = expected_method['name']
            path = expected_method['path']
            http_method = expected_method['http_method']
            
            if method_name in actual_methods:
                print(f"{Colors.GREEN}✅{Colors.END} {method_name}() → Matches {http_method} {path}")
            else:
                violations += 1
                print(f"{Colors.RED}❌{Colors.END} {method_name}() → MISSING (expected from {http_method} {path})")
        
        # Check for unexpected methods (not in OpenAPI)
        expected_names = {m['name'] for m in expected_methods}
        for actual_method in actual_methods:
            if actual_method not in expected_names and not actual_method.startswith('Get') and not actual_method.startswith('Post'):
                # Skip non-REST methods (constructors, Dispose, etc.)
                continue
            
            if actual_method not in expected_names:
                violations += 1
                print(f"{Colors.RED}❌{Colors.END} {actual_method}() → UNEXPECTED (not in OpenAPI spec)")
    
    return violations, total_checks

def main():
    parser = argparse.ArgumentParser(
        description='Validate API naming policy compliance (ADR-002)',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  python validate-api-naming.py --openapi openapi.json --assembly bin/Release/net8.0/MixingStation.Client.dll
  python validate-api-naming.py --openapi spec.json --assembly MixingStation.Client.dll --strict

Exit codes:
  0 - All methods comply
  1 - Violations found
  2 - Script error
        """
    )
    
    parser.add_argument('--openapi', type=Path, required=True, help='Path to OpenAPI spec (JSON)')
    parser.add_argument('--assembly', type=Path, required=True, help='Path to compiled .NET assembly (.dll)')
    parser.add_argument('--strict', action='store_true', help='Fail on warnings (not just errors)')
    
    args = parser.parse_args()
    
    # Parse OpenAPI spec
    print(f"{Colors.BLUE}📖 Parsing OpenAPI spec: {args.openapi}{Colors.END}")
    expected_methods = parse_openapi(args.openapi)
    
    # Extract actual methods from assembly
    print(f"{Colors.BLUE}🔍 Reflecting over assembly: {args.assembly}{Colors.END}")
    actual_methods = extract_actual_methods_from_assembly(args.assembly)
    
    # Validate naming
    violations, total_checks = validate_naming(expected_methods, actual_methods)
    
    # Print summary
    print(f"\n{Colors.BLUE}═══════════════════════════════════════════════{Colors.END}")
    print(f"\nTotal checks: {total_checks}")
    print(f"Violations: {violations}")
    
    if violations == 0:
        print(f"\n{Colors.GREEN}✅ SUCCESS: All methods comply with ADR-002 naming policy{Colors.END}")
        sys.exit(0)
    else:
        print(f"\n{Colors.RED}❌ FAILURE: {violations} naming violations found{Colors.END}")
        print(f"{Colors.YELLOW}💡 Fix: Update method names to match REST endpoints (see ADR-002){Colors.END}")
        sys.exit(1)

if __name__ == '__main__':
    main()
