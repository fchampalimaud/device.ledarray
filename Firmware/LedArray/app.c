#include "hwbp_core.h"
#include "hwbp_core_regs.h"
#include "hwbp_core_types.h"

#include "app.h"
#include "app_funcs.h"
#include "app_ios_and_regs.h"
#include "i2c.h"
#include "fly_pit_boxes.h"

/************************************************************************/
/* Declare application registers                                        */
/************************************************************************/
extern AppRegs app_regs;
extern uint8_t app_regs_type[];
extern uint16_t app_regs_n_elements[];
extern uint8_t *app_regs_pointer[];
extern void (*app_func_rd_pointer[])(void);
extern bool (*app_func_wr_pointer[])(void*);

/************************************************************************/
/* Initialize app                                                       */
/************************************************************************/
static const uint8_t default_device_name[] = "LedArray";

void hwbp_app_initialize(void)
{
   /* Define versions */
   uint8_t hwH = 1;
   uint8_t hwL;
   uint8_t fwH = 2;
   uint8_t fwL = 7;
   uint8_t ass = 0;
   
   io_pin2in(&PORTA, 0, PULL_IO_TRISTATE, SENSE_IO_EDGES_BOTH);         // VERSION0
   io_pin2in(&PORTA, 3, PULL_IO_UP, SENSE_IO_EDGES_BOTH);               // VERSION1
   
   if (read_VERSION0)
   {
      hwL = 1;
   }
   else
   {
      if (read_VERSION1)
      hwL = 2;
      else
      hwL = 3;
   }

   /* Start core */
   core_func_start_core(
      1088,
      hwH, hwL,
      fwH, fwL,
      ass,
      (uint8_t*)(&app_regs),
      APP_NBYTES_OF_REG_BANK,
      APP_REGS_ADD_MAX - APP_REGS_ADD_MIN + 1,
      default_device_name,
	  false,	// The device is _not_ able to repeat the harp timestamp clock
	  false,	// The device is _not_ able to generate the harp timestamp clock
	  0
   );
}

/************************************************************************/
/* Handle if a catastrophic error occur                                 */
/************************************************************************/
void core_callback_catastrophic_error_detected(void)
{
   clr_LED0_PWR_ON;
	clr_LED1_PWR_ON;
	clr_LED0_TRANSISTOR;
	clr_LED1_TRANSISTOR;
   clr_AUX0;
   clr_AUX1;
   clr_BOARD_LED0;
   clr_BOARD_LED1;
}

/************************************************************************/
/* General definitions                                                  */
/************************************************************************/
// #define NBYTES 23

/************************************************************************/
/* General used functions                                               */
/************************************************************************/
uint16_t get_divider(uint8_t prescaler)
{
	switch(prescaler)
	{
		case TIMER_PRESCALER_DIV1: return 1;
		case TIMER_PRESCALER_DIV2: return 2;
		case TIMER_PRESCALER_DIV4: return 4;
		case TIMER_PRESCALER_DIV8: return 8;
		case TIMER_PRESCALER_DIV64: return 64;
		case TIMER_PRESCALER_DIV256: return 256;
		case TIMER_PRESCALER_DIV1024: return 1024;
		default: return 0;
	}
}

bool update_reals(float * real_freq, float * real_dcycle, float freq, float dcycle)
{
	uint8_t prescaler;
	uint16_t target_count;
	
	if (calculate_timer_16bits(32000000, freq, &prescaler, &target_count))
	{
		*real_freq = 32000000.0 / ((uint32_t)(get_divider(prescaler)) * (uint32_t)target_count);
		*real_dcycle = 100.0 * ((float)((uint16_t)(dcycle/100.0 * target_count + 0.5)) / target_count);
		//duty_cycle0 = app_regs.REG_CH0_DUTYCYCLE/100.0 * target_count + 0.5;
		
		if (*real_dcycle <= 0 || *real_dcycle >= 100)
			return false;
	}
	else
	{
		return false;
	}

	return true;
}

static const uint8_t crc_table[] = {
	0x00, 0x07, 0x0e, 0x09, 0x1c, 0x1b, 0x12, 0x15, 0x38, 0x3f, 0x36, 0x31,
	0x24, 0x23, 0x2a, 0x2d, 0x70, 0x77, 0x7e, 0x79, 0x6c, 0x6b, 0x62, 0x65,
	0x48, 0x4f, 0x46, 0x41, 0x54, 0x53, 0x5a, 0x5d, 0xe0, 0xe7, 0xee, 0xe9,
	0xfc, 0xfb, 0xf2, 0xf5, 0xd8, 0xdf, 0xd6, 0xd1, 0xc4, 0xc3, 0xca, 0xcd,
	0x90, 0x97, 0x9e, 0x99, 0x8c, 0x8b, 0x82, 0x85, 0xa8, 0xaf, 0xa6, 0xa1,
	0xb4, 0xb3, 0xba, 0xbd, 0xc7, 0xc0, 0xc9, 0xce, 0xdb, 0xdc, 0xd5, 0xd2,
	0xff, 0xf8, 0xf1, 0xf6, 0xe3, 0xe4, 0xed, 0xea, 0xb7, 0xb0, 0xb9, 0xbe,
	0xab, 0xac, 0xa5, 0xa2, 0x8f, 0x88, 0x81, 0x86, 0x93, 0x94, 0x9d, 0x9a,
	0x27, 0x20, 0x29, 0x2e, 0x3b, 0x3c, 0x35, 0x32, 0x1f, 0x18, 0x11, 0x16,
	0x03, 0x04, 0x0d, 0x0a, 0x57, 0x50, 0x59, 0x5e, 0x4b, 0x4c, 0x45, 0x42,
	0x6f, 0x68, 0x61, 0x66, 0x73, 0x74, 0x7d, 0x7a, 0x89, 0x8e, 0x87, 0x80,
	0x95, 0x92, 0x9b, 0x9c, 0xb1, 0xb6, 0xbf, 0xb8, 0xad, 0xaa, 0xa3, 0xa4,
	0xf9, 0xfe, 0xf7, 0xf0, 0xe5, 0xe2, 0xeb, 0xec, 0xc1, 0xc6, 0xcf, 0xc8,
	0xdd, 0xda, 0xd3, 0xd4, 0x69, 0x6e, 0x67, 0x60, 0x75, 0x72, 0x7b, 0x7c,
	0x51, 0x56, 0x5f, 0x58, 0x4d, 0x4a, 0x43, 0x44, 0x19, 0x1e, 0x17, 0x10,
	0x05, 0x02, 0x0b, 0x0c, 0x21, 0x26, 0x2f, 0x28, 0x3d, 0x3a, 0x33, 0x34,
	0x4e, 0x49, 0x40, 0x47, 0x52, 0x55, 0x5c, 0x5b, 0x76, 0x71, 0x78, 0x7f,
	0x6a, 0x6d, 0x64, 0x63, 0x3e, 0x39, 0x30, 0x37, 0x22, 0x25, 0x2c, 0x2b,
	0x06, 0x01, 0x08, 0x0f, 0x1a, 0x1d, 0x14, 0x13, 0xae, 0xa9, 0xa0, 0xa7,
	0xb2, 0xb5, 0xbc, 0xbb, 0x96, 0x91, 0x98, 0x9f, 0x8a, 0x8d, 0x84, 0x83,
	0xde, 0xd9, 0xd0, 0xd7, 0xc2, 0xc5, 0xcc, 0xcb, 0xe6, 0xe1, 0xe8, 0xef,
	0xfa, 0xfd, 0xf4, 0xf3
};

uint8_t crc8(uint8_t *p, uint8_t len)
{
	uint16_t i;
	uint16_t crc = 0x0;

	while (len--) {
		i = (crc ^ *p++) & 0xFF;
		crc = (crc_table[i] ^ (crc << 8)) & 0xFF;
	}

	return crc & 0xFF;
}

i2c_dev_t dev;
bool write_SMBus_byte(uint8_t add, uint8_t reg, uint8_t byte)
{
	uint8_t crc[3];

	dev.add = add;
	dev.reg = reg;
	
	crc[0] = add<<1;
	crc[1] = reg;
	crc[2] = byte;
	
	dev.data[0] = byte;
	dev.data[1] = crc8(crc, 3);

	return i2c0_wArray(&dev, 2);
}

bool write_SMBus_word(uint8_t add, uint8_t reg, int16_t word)
{
	uint8_t crc[4];

	dev.add = add;
	dev.reg = reg;
	
	crc[0] = add << 1;
	crc[1] = reg;
	crc[2] = *((uint8_t*)(&word));
	crc[3] = *(((uint8_t*)(&word))+1);
	
	dev.data[0] = *((uint8_t*)(&word));
	dev.data[1] = *(((uint8_t*)(&word))+1);
	dev.data[2] = crc8(crc, 4);

	return i2c0_wArray(&dev, 3);
}

/************************************************************************/
/* Initialization Callbacks                                             */
/************************************************************************/
bool SMBus_exist_on_bus0 = false;
bool SMBus_exist_on_bus1 = false;

void core_callback_define_clock_default(void) {}
	
void core_callback_initialize_hardware(void)	
{
	/* Initialize IOs */
	/* Don't delete this function!!! */
	init_ios();
}

void core_callback_reset_registers(void)
{
	/* Initialize registers */
	app_regs.REG_POWER_EN = 0;
	app_regs.REG_LED_BEHAVING = 0;
	app_regs.REG_LED_ON = 0;
   
	app_regs.REG_OUT_CONFIGURATION = GM_OUT0_SOFTWARE | GM_OUT1_SOFTWARE;
	app_regs.REG_IN_CONFIGURATION = GM_IN0_CONF_LED0_ON | GM_IN1_CONF_LED1_ON;
	app_regs.REG_LED_CONFIGURATION = GM_LED0_PWM | GM_LED1_PWM;
	
	app_regs.REG_LED0_SUPPLY_PWR_CONF = 60;
	app_regs.REG_LED1_SUPPLY_PWR_CONF = 60;
	
	app_regs.REG_LED0_PWM_FREQ = 5;				// 5 Hz
	app_regs.REG_LED0_PWM_DCYCLE = 50;			// 50 %
	app_regs.REG_LED0_PWM_PULSES = 30;			// 3 s
	app_regs.REG_LED0_INTERVAL_ON = 25;			// 25 ms
	app_regs.REG_LED0_INTERVAL_OFF = 150;		// 150 ms
	app_regs.REG_LED0_INTERVAL_PULSES = 10;	// 10 pulses
	app_regs.REG_LED0_INTERVAL_TAIL = 2000;	// 2 s
	app_regs.REG_LED0_INTERVAL_REPS = 3;		// 3 repetitions

	
	app_regs.REG_LED1_PWM_FREQ = 5;				// 5 Hz
	app_regs.REG_LED1_PWM_DCYCLE = 50;			// 50 %
	app_regs.REG_LED1_PWM_PULSES = 30;			// 3 s
	app_regs.REG_LED1_INTERVAL_ON = 25;			// 25 ms
	app_regs.REG_LED1_INTERVAL_OFF = 150;		// 150 ms
	app_regs.REG_LED1_INTERVAL_PULSES = 10;	// 10 pulses
	app_regs.REG_LED1_INTERVAL_TAIL = 2000;	// 2 s
	app_regs.REG_LED1_INTERVAL_REPS = 3;		// 3 repetitions

	app_regs.REG_LED0_PWM_FREQ_REAL = 10.0;
	app_regs.REG_LED0_PWM_DCYCLE_REAL = 50.0;
	app_regs.REG_LED1_PWM_FREQ_REAL = 10.0;
	app_regs.REG_LED1_PWM_DCYCLE_REAL = 50.0;

	app_regs.REG_AUX_DIG_OUT = 0;
   
	app_regs.REG_AUX_SUPPLY_PWR_CONF = 60;
   
	app_regs.REG_OUT_STATE = 0;
   
	app_regs.REG_DUMMY0 = 0;

	app_regs.REG_EVNT_ENABLE = B_EVT_IN_STATE | B_EVT_LED_ON;
}

bool bus_expansion_exists;

void core_callback_registers_were_reinitialized(void)
{  
   io_pin2out(&PORTD, 5, OUT_IO_DIGITAL, IN_EN_IO_EN);   // STATE
   
   /* Update Regulators with configured voltage */
   i2c0_init();
   
   bus_expansion_exists = initialize_boxes();
   
   if (write_SMBus_word(17, 0x22, ((int16_t) app_regs.REG_LED0_SUPPLY_PWR_CONF) * 5 -300))
      SMBus_exist_on_bus0 = true;
   
   if(write_SMBus_word(33, 0x22, ((int16_t) app_regs.REG_LED1_SUPPLY_PWR_CONF) * 5 -300))
      SMBus_exist_on_bus1 = true;
   
   write_SMBus_word(25, 0x22, ((int16_t) app_regs.REG_AUX_SUPPLY_PWR_CONF) * 5 -300);
      
   app_regs.REG_LED_BEHAVING = 0;
   app_regs.REG_IN_STATE = 0;
   
   if (read_IN0)
   {
      app_regs.REG_IN_STATE |= B_IN0;
      switch (app_regs.REG_IN_CONFIGURATION & MSK_IN0_CONF)
      {
         case GM_IN0_CONF_LED0_PWR_EN:
            app_regs.REG_POWER_EN |= B_LED0_PWR_EN;
            break;
         case GM_IN0_CONF_LED0_ON:
            app_regs.REG_LED_ON = (app_regs.REG_LED_ON & B_LED1_TO_ON) | B_LED0_TO_ON;
            break;
         case GM_IN0_CONF_LED1_PWR_EN:
            app_regs.REG_POWER_EN |= B_LED1_PWR_EN;
            break;
         case GM_IN0_CONF_LED1_ON:
            app_regs.REG_LED_ON = (app_regs.REG_LED_ON & B_LED0_TO_ON) | B_LED1_TO_ON;
            break;
      }
   }

   if (read_IN1)
   {
      app_regs.REG_IN_STATE |= B_IN1;
      switch (app_regs.REG_IN_CONFIGURATION & MSK_IN1_CONF)
      {
         case GM_IN1_CONF_LED0_PWR_EN:
         app_regs.REG_POWER_EN |= B_LED0_PWR_EN;
         break;
         case GM_IN1_CONF_LED0_ON:
         app_regs.REG_LED_ON = (app_regs.REG_LED_ON & B_LED1_TO_ON) | B_LED0_TO_ON;
         break;
         case GM_IN1_CONF_LED1_PWR_EN:
         app_regs.REG_POWER_EN |= B_LED1_PWR_EN;
         break;
         case GM_IN1_CONF_LED1_ON:
         app_regs.REG_LED_ON = (app_regs.REG_LED_ON & B_LED0_TO_ON) | B_LED1_TO_ON;
         
         break;
      }
   }

   if (app_regs.REG_POWER_EN & B_LED0_PWR_EN)
      set_LED0_PWR_ON;
   if (app_regs.REG_POWER_EN & B_LED1_PWR_EN)
      set_LED1_PWR_ON;

   if (app_regs.REG_LED_ON & B_LED0_TO_ON)
      set_LED0_TRANSISTOR;      
   if (app_regs.REG_LED_ON & B_LED1_TO_ON)
      set_LED1_TRANSISTOR;
   
   if (read_LED0_PWR_ON && read_LED0_TRANSISTOR)
      if (core_bool_is_visual_enabled())
         set_BOARD_LED0;
         
   if (read_LED1_PWR_ON && read_LED1_TRANSISTOR)
      if (core_bool_is_visual_enabled())
         set_BOARD_LED1;
         
   if ((app_regs.REG_OUT_CONFIGURATION & MSK_OUT0_CONF) == GM_OUT0_SOFTWARE)
      if (app_regs.REG_OUT_STATE & B_OUT0_TO_HIGH)
         set_OUT0; else clr_OUT0;
   
   if ((app_regs.REG_OUT_CONFIGURATION & MSK_OUT1_CONF) == GM_OUT1_SOFTWARE)
      if (app_regs.REG_OUT_STATE & B_OUT1_TO_HIGH)
         set_OUT1; else clr_OUT1;
   
   if ((app_regs.REG_OUT_CONFIGURATION & MSK_OUT0_CONF) == GM_OUT0_LED0_PWR_EN)
      if (read_LED0_PWR_ON)
         set_OUT0; else clr_OUT0;
   
   if ((app_regs.REG_OUT_CONFIGURATION & MSK_OUT1_CONF) == GM_OUT1_LED1_PWR_EN)
      if (read_LED1_PWR_ON)
         set_OUT1; else clr_OUT1;
   
   if ((app_regs.REG_OUT_CONFIGURATION & MSK_OUT0_CONF) == GM_OUT0_LED0_START)
      if (app_regs.REG_LED_BEHAVING & B_LED0_START)
         set_OUT0; else clr_OUT0;
   
   if ((app_regs.REG_OUT_CONFIGURATION & MSK_OUT1_CONF) == GM_OUT1_LED1_START)
      if (app_regs.REG_LED_BEHAVING & B_LED1_START)
         set_OUT1; else clr_OUT1;
   
   if ((app_regs.REG_OUT_CONFIGURATION & MSK_OUT0_CONF) == GM_OUT0_LED0_ON)
      if (read_LED0_TRANSISTOR)
         set_OUT0; else clr_OUT0;
   
   if ((app_regs.REG_OUT_CONFIGURATION & MSK_OUT1_CONF) == GM_OUT1_LED1_ON)
      if (read_LED1_TRANSISTOR)
         set_OUT1; else clr_OUT1;
   
   update_reals(&app_regs.REG_LED0_PWM_FREQ_REAL, &app_regs.REG_LED0_PWM_DCYCLE_REAL, app_regs.REG_LED0_PWM_FREQ, app_regs.REG_LED0_PWM_DCYCLE);
   update_reals(&app_regs.REG_LED1_PWM_FREQ_REAL, &app_regs.REG_LED1_PWM_DCYCLE_REAL, app_regs.REG_LED1_PWM_FREQ, app_regs.REG_LED1_PWM_DCYCLE);
}

/************************************************************************/
/* Callbacks: Visualization                                             */
/************************************************************************/
void core_callback_visualen_to_on(void)
{
	/* Update channels enable indicators */
	if ((app_regs.REG_POWER_EN & B_LED0_PWR_EN) && (app_regs.REG_LED_ON & B_LED0_TO_ON))
      set_BOARD_LED0;
   if ((app_regs.REG_POWER_EN & B_LED1_PWR_EN) && (app_regs.REG_LED_ON & B_LED1_TO_ON))
      set_BOARD_LED1;
}

void core_callback_visualen_to_off(void)
{
	/* Clear all the enabled indicators */
	clr_BOARD_LED0;
	clr_BOARD_LED1;
}

/************************************************************************/
/* Callbacks: Change on the operation mode                              */
/************************************************************************/
void core_callback_device_to_standby(void) {}
void core_callback_device_to_active(void) {}
void core_callback_device_to_enchanced_active(void) {}
void core_callback_device_to_speed(void) {}

/************************************************************************/
/* Callbacks: 1 ms timer                                                */
/************************************************************************/
uint16_t SMBus_exist_on_bus0_counter = 500;
uint16_t SMBus_exist_on_bus1_counter = 1500;

void core_callback_t_before_exec(void) {}
void core_callback_t_after_exec(void) {}
void core_callback_t_new_second(void) {}
void core_callback_t_500us(void) {}
void core_callback_t_1ms(void)
{
   if (SMBus_exist_on_bus0 == false)
   {
      if (++SMBus_exist_on_bus0_counter == 3000)
      {
         SMBus_exist_on_bus0_counter = 0;
         
         if (write_SMBus_word(17, 0x22, ((int16_t) app_regs.REG_LED0_SUPPLY_PWR_CONF) * 5 -300))
            SMBus_exist_on_bus0 = true;
      }
   }
   
   if (SMBus_exist_on_bus1 == false)
   {
      if (++SMBus_exist_on_bus1_counter == 3000)
      {
         SMBus_exist_on_bus1_counter = 0;
         
         if(write_SMBus_word(33, 0x22, ((int16_t) app_regs.REG_LED1_SUPPLY_PWR_CONF) * 5 -300))
            SMBus_exist_on_bus1 = true;
      }
   }
}

/************************************************************************/
/* Callbacks: clock control                                              */
/************************************************************************/
void core_callback_clock_to_repeater(void) {}
void core_callback_clock_to_generator(void) {}
void core_callback_clock_to_unlock(void) {}
void core_callback_clock_to_lock(void) {}
	
/************************************************************************/
/* Callbacks: uart control                                              */
/************************************************************************/
void core_callback_uart_rx_before_exec(void) {}
void core_callback_uart_rx_after_exec(void) {}
void core_callback_uart_tx_before_exec(void) {}
void core_callback_uart_tx_after_exec(void) {}
void core_callback_uart_cts_before_exec(void) {}
void core_callback_uart_cts_after_exec(void) {}

/************************************************************************/
/* Callbacks: Read app register                                         */
/************************************************************************/
bool core_read_app_register(uint8_t add, uint8_t type)
{
	/* Check if it will not access forbidden memory */
	if (add < APP_REGS_ADD_MIN || add > APP_REGS_ADD_MAX)
		return false;
	
	/* Check if type matches */
	if (app_regs_type[add-APP_REGS_ADD_MIN] != type)
		return false;
	
	/* Receive data */
	(*app_func_rd_pointer[add-APP_REGS_ADD_MIN])();	

	/* Return success */
	return true;
}

/************************************************************************/
/* Callbacks: Write app register                                        */
/************************************************************************/
bool core_write_app_register(uint8_t add, uint8_t type, uint8_t * content, uint16_t n_elements)
{
	/* Check if it will not access forbidden memory */
	if (add < APP_REGS_ADD_MIN || add > APP_REGS_ADD_MAX)
		return false;
	
	/* Check if type matches */
	if (app_regs_type[add-APP_REGS_ADD_MIN] != type)
		return false;

	/* Check if the number of elements matches */
	if (app_regs_n_elements[add-APP_REGS_ADD_MIN] != n_elements)
		return false;

	/* Process data and return false if write is not allowed or contains errors */
	return (*app_func_wr_pointer[add-APP_REGS_ADD_MIN])(content);
}