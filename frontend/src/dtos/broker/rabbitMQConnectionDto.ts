export interface RabbitMQConnectionDto {
    managementPort: number;
    managementTls: boolean;
    amqpPort: number;
    amqpTls: boolean;
    host: string;
    vHost: string;
}
