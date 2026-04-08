using GUtility.MotorController;
using GUtility.MotorController.MotorController;
using System;
using System.Threading;

namespace MotorTestDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            MotorControllerManager motor = null;

            try
            {
                // 1. 建立設定
                MotorConfig config = new MotorConfig();
                config.PortName = "COM3";
                config.AxisCount = 2;

                config.DefaultMoveSpeed = 200;
                config.DefaultAcceleration = 2000;
                config.DefaultDeceleration = 2000;

                config.IdlePollingIntervalMs = 500;
                config.MovingPollingIntervalMs = 100;

                // 2. 建立控制器
                motor = new MotorControllerManager(config);

                // 3. 連線
                bool ok = motor.Connect();
                if (!ok)
                {
                    Console.WriteLine("馬達控制器連線失敗");
                    return;
                }

                Console.WriteLine("馬達控制器連線成功");

                // 4. 先更新一次完整狀態
                motor.RefreshFull();
                

                // 假設使用 Axis 0
                int axis = 0;

                // 5. 讀取目前狀態
                MotorAxisState axisState = motor.GetAxisState(axis);
                if (axisState == null)
                {
                    Console.WriteLine("無法取得軸狀態");
                    return;
                }

                Console.WriteLine("Axis{0} 目前位置: {1}", axis, axisState.CurrentPosition);
                Console.WriteLine("Axis{0} ServoOn: {1}", axis, axisState.IsServoOn);
                Console.WriteLine("Axis{0} Alarm: {1}", axis, axisState.HasAlarm);

                // 6. 若有 Alarm，先清除
                if (axisState.HasAlarm)
                {
                    Console.WriteLine("Axis{0} 有 Alarm，先清除", axis);
                    motor.ClearAlarm(axis);
                    Thread.Sleep(300);
                    motor.RefreshFull();

                    axisState = motor.GetAxisState(axis);
                    if (axisState.HasAlarm)
                    {
                        Console.WriteLine("Alarm 清除失敗，停止流程");
                        return;
                    }
                }

                // 7. Servo ON
                if (!axisState.IsServoOn)
                {
                    Console.WriteLine("Axis{0} Servo ON", axis);
                    motor.ServoOn(axis);
                    Thread.Sleep(300);
                    motor.RefreshLite();

                    axisState = motor.GetAxisState(axis);
                    if (!axisState.IsServoOn)
                    {
                        Console.WriteLine("Servo ON 失敗");
                        return;
                    }
                }

                // 8. 移動到絕對位置
                int targetPos = 10000;
                Console.WriteLine("Axis{0} 移動到絕對位置: {1}", axis, targetPos);

                motor.MoveAbs(axis, targetPos, 200);

                // 9. 輪詢等待移動完成
                while (true)
                {
                    motor.RefreshLite();
                    axisState = motor.GetAxisState(axis);

                    Console.WriteLine("Pos={0}, Moving={1}, Alarm={2}",
                        axisState.CurrentPosition,
                        axisState.IsMoving,
                        axisState.HasAlarm);

                    if (axisState.HasAlarm)
                    {
                        Console.WriteLine("移動中發生 Alarm，停止");
                        motor.Stop(axis);
                        break;
                    }

                    if (!axisState.IsMoving)
                    {
                        Console.WriteLine("移動完成");
                        break;
                    }

                    Thread.Sleep(100);
                }

                // 10. 再做一次相對移動
                Console.WriteLine("Axis{0} 相對移動 +2000", axis);
                motor.MoveRel(axis, 2000, 150);

                while (true)
                {
                    motor.RefreshLite();
                    axisState = motor.GetAxisState(axis);

                    Console.WriteLine("Pos={0}, Moving={1}", axisState.CurrentPosition, axisState.IsMoving);

                    if (axisState.HasAlarm)
                    {
                        Console.WriteLine("相對移動中 Alarm，停止");
                        motor.Stop(axis);
                        break;
                    }

                    if (!axisState.IsMoving)
                    {
                        Console.WriteLine("相對移動完成");
                        break;
                    }

                    Thread.Sleep(100);
                }

                // 11. Servo OFF（看需求，不一定每次都要關）
                Console.WriteLine("Axis{0} Servo OFF", axis);
                motor.ServoOff(axis);

                Console.WriteLine("流程完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine("發生例外: " + ex.Message);
            }
            finally
            {
                if (motor != null)
                {
                    motor.Disconnect();
                    motor.Dispose();
                }

                Console.WriteLine("已斷線並釋放資源");
            }
        }
    }
}