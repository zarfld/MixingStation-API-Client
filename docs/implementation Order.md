Recommended Implementation Order (Priority Phases)
Phase 3: App Mixer Lifecycle (5 endpoints, PRIORITY: HIGH)
Complete mixer management for basic app lifecycle:

GET /app/mixers/available
POST /app/mixers/search
GET /app/mixers/searchResults
POST /app/mixers/disconnect
POST /app/mixers/offline
Why first? Completes core mixer connection lifecycle (search → connect → disconnect)

Phase 4: Console Data Discovery (6 endpoints, PRIORITY: HIGH)
Essential for understanding mixer data structure:

GET /console/data/categories
GET /console/data/paths
GET /console/data/paths/{path}
GET /console/data/definitions/{path}
GET /console/data/definitions2/{path}
POST /console/data/unsubscribe
Why second? Enables dynamic discovery of mixer capabilities before manipulating data

Phase 5: Console Authentication & Mix Targets (3 endpoints, PRIORITY: MEDIUM)
GET /console/auth/info
POST /console/auth/login
GET /console/mixTargets
Why third? Required for secured mixers and understanding routing architecture

Phase 6: Console Metering (3 endpoints, PRIORITY: MEDIUM)
POST /console/metering/subscribe
POST /console/metering/unsubscribe
POST /console/metering2/subscribe
Why fourth? Real-time level monitoring (useful but not critical for basic operation)

Phase 7: App Presets (6 endpoints, PRIORITY: LOW)
GET /app/presets/scopes
POST /app/presets/channel/apply
POST /app/presets/channel/create
POST /app/presets/scenes/apply
POST /app/presets/scenes/create
GET /app/presets/lastError
Why later? Advanced feature, depends on stable core functionality

Phase 8: App IDCA & UI (6 endpoints, PRIORITY: LOW)
GET /app/idcas
GET /app/idcas/{index}
POST /app/idcas/{index}/delete
POST /app/idcas/rearrange
GET /app/ui/selectedChannel
POST /app/ui/selectedChannel/{nameOrIndex}
Why later? Advanced group management and UI state synchronization

Phase 9: App Network & Misc (3 endpoints, PRIORITY: LOW)
GET /app/network/interfaces
GET /app/network/interfaces/primary
POST /app/save
Why last? Network diagnostics and state persistence (nice-to-have)

Phase 10: Console Config Events (1 endpoint, PRIORITY: LOW)
GET /console/onConfigChanged
Why last? Event notifications, likely requires WebSocket handling