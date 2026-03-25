@Library('hobom-shared-lib') _
hobomPipeline(
  serviceName:    'dev-hammer-support',
  hostPort:       '5004',
  containerPort:  '8080',
  memory:         '512m',
  cpus:           '0.5',
  envPath:        '/etc/hobom-dev/dev-hammer-support/.env',
  addHost:        true,
  submodules:     false,
  smokeCheckPath: '/health'
)
